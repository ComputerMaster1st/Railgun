Imports System.Text
Imports AudioChord
Imports Discord
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Music.PlayerEventArgs
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Core.Managers
    
    Public Class PlayerManager
        
        Private ReadOnly _log As Log
        Private ReadOnly _client As IDiscordClient
        Private ReadOnly _commandUtils As CommandUtils
        Private ReadOnly _services As IServiceProvider
        Private ReadOnly _musicService As MusicService

        Public ReadOnly Property PlayerContainers As New List(Of PlayerContainer)
        
        Public Sub New(services As IServiceProvider)
            _log = services.GetService(Of Log)
            _client = services.GetService(Of IDiscordClient)
            _commandUtils = services.GetService(Of CommandUtils)
            _musicService = services.GetService(Of MusicService)
            _services = services
        End Sub
        
        Private Async Function PlayerConnectedAsync(args As PlayerConnectedEventArgs) As Task
            Dim tc As ITextChannel = PlayerContainers.First(
                Function(container) container.GuildId = args.GuildId).TextChannel
            Await _log.LogToBotLogAsync($"<{tc.Guild.Name} ({tc.GuildId})> Connected!", BotLogType.MusicPlayer)
        End Function
        
        Private Async Function PlayerCurrentlyPlayingAsync(args As PlayerCurrentlyPlayingEventArgs) As Task
            Try
                Dim data As ServerMusic
                Dim tc As ITextChannel
                
                Using scope As IServiceScope = _services.CreateScope()
                    Dim db As TreeDiagramContext = scope.ServiceProvider.GetService(Of TreeDiagramContext)
                    data = Await db.ServerMusics.GetAsync(args.GuildId)
                End Using
                
                If data.NowPlayingChannel <> 0
                    Dim guild As IGuild = Await _client.GetGuildAsync(args.GuildId)
                    tc = Await guild.GetTextChannelAsync(data.NowPlayingChannel)
                Else 
                    tc = PlayerContainers.First(Function(container) container.GuildId = args.GuildId).TextChannel
                End If
                
                If Not (data.SilentNowPlaying)
                    Dim output As New StringBuilder
                    
                    output.AppendFormat("Now Playing: {0} || ID: {1}", Format.Bold(args.Metadata.Name), 
                                        Format.Bold(args.SongId)).AppendLine() _
                        .AppendFormat("Time: {0} || Uploader: {1} || URL: {2}", 
                                      Format.Bold(args.Metadata.Length.ToString()), 
                                      Format.Bold(args.Metadata.Uploader), Format.Bold($"<{args.Metadata.Url}>"))
                    
                    Await tc.SendMessageAsync(output.ToString())
                End If
                
                Await _log.LogToBotLogAsync($"<{tc.Guild.Name} ({tc.GuildId})> Now Playing {args.SongId}", 
                                            BotLogType.MusicPlayer)
            Catch
                _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", 
                                                      $"{args.GuildId} Missing TC!")).GetAwaiter()
                PlayerContainers.First(Function(container) container.GuildId = args.GuildId).Player.CancelStream()
            End Try
        End Function
        
        Private Async Function PlayerTimeoutAsync(args As PlayerTimeoutEventArgs) As Task
            Dim tc As ITextChannel = PlayerContainers.First(
                Function(container) container.GuildId = args.GuildId).TextChannel
            
            Try
                Await tc.SendMessageAsync("Connection to Discord Voice has timed out! Please try again.")
            Catch
                _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", 
                                                      $"{args.GuildId} Missing TC!")).GetAwaiter()
            Finally
                Dim output As New StringBuilder
                
                output.AppendFormat("<{0} ({1})> Action Timeout!", tc.Guild.Name, args.GuildId).AppendLine() _
                    .AppendFormat("---- Exception : {0}", args.Exception.ToString())
                
                _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicPlayer).GetAwaiter()
            End Try
        End Function

        Private Async Function PlayerFinishedAsync(args As PlayerFinishedEventArgs) As Task
            Dim playerContainer As PlayerContainer = PlayerContainers.FirstOrDefault(
                Function(container) container.GuildId = args.GuildId)
            
            If playerContainer Is Nothing Then Return
            
            Dim tc = playerContainer.TextChannel
            
            Try
                Dim output As New StringBuilder
                
                If args.Exception IsNot Nothing 
                    Await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Error, "Music", 
                                                                $"{tc.GuildId} Exception!", args.Exception))
                    
                    Dim logOutput As New StringBuilder
                    
                    logOutput.AppendFormat("<{0} ({1})> Music Player Error!", tc.Guild.Name, tc.GuildId).AppendLine() _
                        .AppendFormat("---- Error : {0}", args.Exception.ToString())
                    
                    Await _log.LogToBotLogAsync(logOutput.ToString(), BotLogType.MusicPlayer)
                    output.AppendLine("An error has occured while playing! The stream has been automatically reset. You may start playing music again at any time.")
                End If
                
                Dim autoOutput As String = If(args.AutoDisconnected, "Auto-", "")
                
                output.AppendFormat("{0}Left Voice Channel", autoOutput)
                
                Await tc.SendMessageAsync(output.ToString())
                Await _log.LogToBotLogAsync($"<{tc.Guild.Name} ({tc.GuildId})> {autoOutput}Disconnected", BotLogType.MusicPlayer)
            Catch
                _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", 
                                                      $"{args.GuildId} Missing TC!")).GetAwaiter()
                _log.LogToBotLogAsync($"<{tc.Guild.Name} ({args.GuildId})> Crash-Disconnected", 
                                      BotLogType.MusicPlayer).GetAwaiter()
            Finally
                StopPlayerAsync(args.GuildId, args.AutoDisconnected).GetAwaiter()
            End Try
        End Function

        Public Async Function CreatePlayerAsync(user As IGuildUser, vc As IVoiceChannel, tc As ITextChannel, 
                                                Optional autoJoin As Boolean = False, 
                                                Optional preRequestedSong As ISong = Nothing) As Task
            Dim playlist As Playlist
            
            Using scope As IServiceScope = _services.CreateScope()
                Dim db As TreeDiagramContext = scope.ServiceProvider.GetService(Of TreeDiagramContext)
                Dim data As ServerMusic = Await db.ServerMusics.GetOrCreateAsync(tc.GuildId)
                playlist = Await _commandUtils.GetPlaylistAsync(data)
            End Using
            
            If playlist.Songs.Count < 1
                If preRequestedSong IsNot Nothing AndAlso Not (playlist.Songs.Contains(preRequestedSong.Id)) Then _ 
                    playlist.Songs.Add(preRequestedSong.Id)
                
                Await tc.SendMessageAsync("As this server has no music yet, I've decided to gather 100 random songs from my repository. One momemt please...")
                
                Dim repository As IEnumerable(Of ISong) = (Await _musicService.GetAllSongsAsync()).ToList()
                Dim rand As New Random
                
                While playlist.Songs.Count < 100
                    Dim i As Integer = rand.Next(0, repository.Count())
                    Dim song As ISong = repository.ElementAtOrDefault(i)
                    
                    If song IsNot Nothing AndAlso Not (playlist.Songs.Contains(song.Id)) Then _
                        playlist.Songs.Add(song.Id)
                End While
                
                Await _musicService.Playlist.UpdateAsync(playlist)
            End If
            
            Dim username As String = Await _commandUtils.GetUsernameOrMentionAsync(user) 
            
            Await tc.SendMessageAsync($"{If(autoJoin, "Music Auto-Join triggered by", "Joining now")} { _
                Format.Bold(username)}. Standby...")
            
            Dim player As New Player(_musicService, vc)
            
            AddHandler player.Connected, Async Sub(s, a) Await PlayerConnectedAsync(a)
            AddHandler player.Playing, Async Sub(s, a) Await PlayerCurrentlyPlayingAsync(a)
            AddHandler player.Timeout, Async Sub(s, a) Await PlayerTimeoutAsync(a)
            AddHandler player.Finished, Async Sub(s, a) Await PlayerFinishedAsync(a)
            
            If preRequestedSong IsNot Nothing
                player.AddSongRequest(preRequestedSong)
                player.AutoSkipped = True
            End If
            
            player.StartPlayer(playlist.Id, autoJoin)
            PlayerContainers.Add(New PlayerContainer(tc, player))
            
            Dim autoString As String = $"{If(autoJoin, "Auto-", "")}Connecting..."
            
            Await _log.LogToBotLogAsync($"<{vc.Guild.Name} ({vc.GuildId})> {autoString}", BotLogType.MusicPlayer)
            Await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "Music", autoString))
        End Function
        
        Public Sub DisconnectPlayer(playerId As ULong)
            PlayerContainers.First(Function(container) container.GuildId = playerId).Player.CancelStream()
        End Sub
        
        Private Async Function StopPlayerAsync(playerId As ULong, Optional autoLeave As Boolean = False) As Task
            Dim playerContainer As PlayerContainer = PlayerContainers.FirstOrDefault(
                Function(container) container.GuildId = playerId)
            
            If playerContainer Is Nothing Then Return
            
            Dim player As Player = playerContainer.Player
            
            If Not (autoLeave) Then player.CancelStream()
            
            While player.Task.Status = TaskStatus.WaitingForActivation
                Await Task.Delay(500)
            End While
            
            player.Task.Dispose()
            PlayerContainers.Remove(PlayerContainers.First(Function(container) container.GuildId = playerId))
            
            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "Music", $"Player ID '{playerId}' Destroyed"))
        End Function
        
        Public Function GetPlayer(playerId As ULong) As PlayerContainer
            Return PlayerContainers.FirstOrDefault(Function(container) container.GuildId = playerId)
        End Function
        
        Public Function IsCreated(playerId As ULong) As Boolean
            If PlayerContainers.Where(Function(container) container.GuildId = playerId).Count > 0 Then Return True
            Return False
        End Function
        
    End Class
    
End NameSpace