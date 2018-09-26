Imports System.IO
Imports AudioChord
Imports Discord
Imports Discord.Audio
Imports MongoDB.Bson
Imports RailgunVB.Core.Music.PlayerEventArgs

Namespace Core.Music
    
    Public Class Player
        
        Private ReadOnly _musicService As MusicService
        Private ReadOnly _guildId As ULong
        Private _playlistId As ObjectId = ObjectId.Empty
        
        Private _client As IAudioClient = Nothing
        Private _audioDisconnected As Boolean = False
        Private _audioDisconnectException As Exception = Nothing
        Private _autoJoined As Boolean = False
        Private _autoDisconnected As Boolean = False
        Private _musicCancelled As Boolean = False
        Private _streamCancelled As Boolean = False
        
        Private ReadOnly _playedSongs As New List(Of String)
        
        Public Property AutoSkipped As Boolean = False
        Public ReadOnly Property CreatedAt As DateTime = DateTime.Now
        Public Property RepeatSong As Boolean = False
        Public ReadOnly Property Requests As New List(Of String)
        Public ReadOnly Property SongStartedAt As DateTime = Nothing
        Public ReadOnly Property Status As PlayerStatus = PlayerStatus.Idle
        Public ReadOnly Property Task As Task = Nothing
        Public ReadOnly Property VoiceChannel As IVoiceChannel
        Public ReadOnly Property VoteSkipped As New List(Of ULong)
        
        Public Event Connected As EventHandler(Of PlayerConnectedEventArgs)
        Public Event Playing As EventHandler(Of PlayerCurrentlyPlayingEventArgs)
        Public Event Timeout As EventHandler(Of PlayerTimeoutEventArgs)
        Public Event Finished As EventHandler(Of PlayerFinishedEventArgs)

        Public ReadOnly Property Latency As Integer
            Get
                If _client IsNot Nothing Then Return _client.Latency
                Return -1
            End Get
        End Property
        
        Public Sub New(musicService As MusicService, vc As IVoiceChannel)
            _musicService = musicService
            VoiceChannel = vc
            _guildId = vc.GuildId
        End Sub
        
        Public Sub CancelMusic()
            _musicCancelled = True
        End Sub
        
        Public Sub CancelStream(Optional audioDisconnected As Boolean = False)
            _musicCancelled = True
            _streamCancelled = True
            _audioDisconnected = audioDisconnected
        End Sub
        
        Public Function AddSongRequest(songId As String) As Boolean
            If Not (Requests.Contains(songId))
                Requests.Add(songId)
                Return True
            End If
            
            Return False
        End Function
        
        Public Function GetFirstSongRequest() As String
            If Requests.Count > 0 Then Return Requests(0)
            Return String.Empty
        End Function
        
        Public Sub RemoveSongRequest(songId As String)
            If Requests.Contains(songId) Then Requests.Remove(songId)
        End Sub

        Public Function VoteSkip(userId As ULong) As Boolean
            If Not (VoteSkipped.Contains(userId))
                VoteSkipped.Add(userId)
                Return True
            End If
            
            Return False
        End Function
        
        Public Sub StartPlayer(playlistId As ObjectId, autoJoin As Boolean)
            _playlistId = playlistId
            _autoJoined = autoJoin
            _Task = Task.Run(New Action(AddressOf StartAsync))
        End Sub
        
        Public Async Function GetUserCountAsync() As Task(Of Integer)
            Dim users As IEnumerable(Of IGuildUser) = Await VoiceChannel.GetUsersAsync().FlattenAsync()

            Return users.Count(Function(user) Not (user.IsBot))
        End Function
        
        Private Async Function QueueSongAsync() As Task(Of String)
            Dim rand As New Random()
            Dim playlist As Playlist = Await _musicService.GetPlaylistAsync(_playlistId)
            Dim request As String = String.Empty
            
            If playlist Is Nothing OrElse playlist.Songs.Count < 1
                Return Nothing
            ElseIf _playedSongs.Count >= playlist.Songs.Count
                _playedSongs.Clear()
            End If
            
            Dim requestedSong As String = GetFirstSongRequest()
            
            If Not (String.IsNullOrEmpty(requestedSong)) Then Return requestedSong
            
            While String.IsNullOrEmpty(request)
                Try
                    request = playlist.Songs(rand.Next(0, playlist.Songs.Count))
                    
                    If Not (String.IsNullOrEmpty(request)) AndAlso _playedSongs.Contains(request) Then _
                        request = String.Empty
                Catch
                    request = String.Empty
                End Try
            End While
            
            Return request
        End Function
        
        Private Async Function IsAloneAsync() As Task(Of Boolean)
            Dim count As Integer = Await GetUserCountAsync()
            
            If count < 1 Then Return True
            Return False
        End Function
        
        Private Async Function TimeoutAsync(timeoutTask As Task, ms As Integer, errorMsg As String) As Task
            Await Task.WhenAny(timeoutTask, Task.Delay(ms))
            
            If Not (timeoutTask.IsCompleted) Then _
                Throw New TimeoutException(
                    $"{errorMsg} (Task Status : {timeoutTask.Status.ToString()})", timeoutTask.Exception)
        End Function
        
        Private Function AudioDisconnected(ex As Exception) As Task
            If Not (_autoDisconnected) AndAlso Not (_streamCancelled)
                _audioDisconnectException = ex
                CancelStream(True)
            End If
            Return Task.CompletedTask
        End Function
        
        Private Async Function ConnectToVoiceAsync() As Task
            _client = Await VoiceChannel.ConnectAsync()
            
            If _client Is Nothing Then _ 
                Throw New TimeoutException(
                    "Unable to establish a connection to voice server! Try changing regions if this problem persists.")
        End Function
        
        Private Async Function StartAsync() As Task
            Dim ex As Exception = Nothing
            
            _Status = PlayerStatus.Connecting
            
            Try
                Await ConnectToVoiceAsync()
                Connected?.Invoke(Me, New PlayerConnectedEventArgs(_guildId))
                AddHandler _client.Disconnected, AddressOf AudioDisconnected
                
                Using client = _client, discordStream As AudioOutStream = client.CreateOpusStream()
                    _Status = PlayerStatus.Connected
                    
                    While Not (_streamCancelled)
                        If Await IsAloneAsync()
                            _autoDisconnected = True
                            Exit While
                        ElseIf _musicCancelled
                            _musicCancelled = False
                        End If
                        
                        _Status = PlayerStatus.Queuing
                        
                        If Requests.Count < 1 Then AddSongRequest(Await QueueSongAsync())
                        
                        Dim song As Song = Await _musicService.GetSongAsync(GetFirstSongRequest())
                        Playing?.Invoke(Me, New PlayerCurrentlyPlayingEventArgs(_guildId, song.Id, song.Metadata))
                        
                        Using databaseStream As Stream = Await song.GetMusicStreamAsync(), 
                            opusStream = New OpusOggReadStream(databaseStream)
                            _Status = PlayerStatus.Playing
                            _SongStartedAt = DateTime.Now
                            
                            Dim bytes As Byte()
                            
                            While opusStream.HasNextPacket AndAlso Not (_musicCancelled)
                                bytes = opusStream.RetrieveNextPacket()
                                Await TimeoutAsync(discordStream.WriteAsync(bytes, 0, bytes.Length), 5000, 
                                                   "WriteAsync has timed out!")
                            End While
                            
                            _Status = PlayerStatus.Finishing
                            Await TimeoutAsync(discordStream.FlushAsync(), 5000, "FlushAsync has timed out!")
                        End Using
                        
                        If RepeatSong 
                            RepeatSong = False
                        Else
                            RemoveSongRequest(song.Id)
                        End If
                        
                        If AutoSkipped AndAlso Requests.Count < 1 Then AutoSkipped = False
                        If VoteSkipped.Count > 0 Then VoteSkipped.Clear()
                    End While
                    
                    _Status = PlayerStatus.Disconnecting
                End Using
            Catch timeEx As TimeoutException
                _Status = PlayerStatus.Timeout
                Timeout?.Invoke(Me, New PlayerTimeoutEventArgs(_guildId, timeEx))
            Catch inEx As Exception
                _Status = PlayerStatus.FailSafe
                ex = inEx
            Finally
                If _audioDisconnected
                    ex = New Exception("AudioClient Unexpected Disconnect!", _audioDisconnectException)
                    _autoDisconnected = False
                End If
                
                Finished?.Invoke(Me, New PlayerFinishedEventArgs(_guildId, _autoDisconnected, ex))
                _Status = PlayerStatus.Disconnected
            End Try
        End Function
        
    End Class
    
End NameSpace