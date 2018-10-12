Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Api.Youtube
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music

    Partial Public Class Music
    
        <Group("play")>
        Public Class MusicPlay
            Inherits ModuleBase
            
            Private ReadOnly _config As MasterConfig
            Private ReadOnly _log As Log
            Private ReadOnly _commandUtils As CommandUtils
            Private ReadOnly _playerManager As PlayerManager
            Private ReadOnly _dbContext As TreeDiagramContext
            Private ReadOnly _musicService As MusicService

            Public Sub New(config As MasterConfig, log As Log, commandUtils As CommandUtils, 
                           playerManager As PlayerManager, dbContext As TreeDiagramContext, 
                           musicService As MusicService)
                _config = config
                _log = log
                _commandUtils = commandUtils
                _playerManager = playerManager
                _dbContext = dbContext
                _musicService = musicService
            End Sub
            
            Private Async Function QueueSongAsync(playerContainer As PlayerContainer, playlist As Playlist, song As ISong, 
                                                  data As ServerMusic, response As IUserMessage) As Task
                Dim nowInstalled = False
                
                If Not (playlist.Songs.Contains(song.Id))
                    playlist.Songs.Add(song.Id)
                    Await _musicService.Playlist.UpdateAsync(playlist)
                    nowInstalled = True
                End If
                
                Dim output As New StringBuilder
                
                output.AppendFormat("{0} Queued {1} as requested by {2}. {3}", 
                    If(nowInstalled, "Installed &", ""), Format.Bold(song.Metadata.Name), 
                    Format.Bold(await _commandUtils.GetUsernameOrMentionAsync(Context.User)), 
                    If(playerContainer Is Nothing, "Now starting music player...", "")).AppendLine()
                
                Dim user As IGuildUser = Context.User
                Dim vc As IVoiceChannel = user.VoiceChannel
                
                If playerContainer Is Nothing
                    Await response.ModifyAsync(Sub(x) x.Content = output.ToString())
                    Await _playerManager.CreatePlayerAsync(user, vc, Context.Channel, 
                                                           preRequestedSong := song)
                    Return
                EndIf 
                
                Dim player As Player = playerContainer.Player
                
                If player.VoiceChannel.Id <> vc.Id
                    Await response.ModifyAsync(Sub(x) x.Content = 
                        "Please be in the same voice channel as me when requesting a song to play.")
                    Return
                End If
                
                player.AddSongRequest(song)
                
                If data.AutoSkip AndAlso Not (player.AutoSkipped)
                    output.AppendLine("Auto-Skipping current song as requested.")
                    player.AutoSkipped = True
                    player.CancelMusic()
                End If
                
                Await response.ModifyAsync(Sub(x) x.Content = output.ToString())
            End Function
            
            <Command>
            Public Async Function PlayAsync(<Remainder> Optional input As String = Nothing) As Task
                If String.IsNullOrWhiteSpace(input) AndAlso Context.Message.Attachments.Count < 1
                    await ReplyAsync("Please specify either a YouTube Link, Music Id, Search Query or upload an audio file.")
                    Return
                Elseif CType(Context.User, IGuildUser).VoiceChannel Is Nothing
                    await ReplyAsync("You're not in a voice channel! Please join one.")
                    Return
                End If
                
                Dim playerContainer As PlayerContainer = _playerManager.GetPlayer(Context.Guild.Id)
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                Await _dbContext.SaveChangesAsync()
                Dim playlist As Playlist = Await _commandUtils.GetPlaylistAsync(data)
                Dim response As IUserMessage = await ReplyAsync("Standby...")
                
                If Context.Message.Attachments.Count > 0
                    Await UploadAsync(playerContainer, playlist, data, response)
                ElseIf input.Contains("YOUTUBE#") OrElse input.Contains("DISCORD#")
                    Await AddByIdAsync(input, playerContainer, playlist, data, response)
                ElseIf input.Contains("http://") OrElse input.Contains("https//")
                    Await AddByUrlAsync(input.Trim("<"c, ">"c), playerContainer, playlist, data, response)
                Else
                    await SearchAsync(input, playerContainer, playlist, data, response)
                End If
            End Function
            
            Private Async Function UploadAsync(playerContainer As PlayerContainer, playlist As Playlist, data As ServerMusic, 
                                               response As IUserMessage) As Task
                Try
                    Dim attachment As IAttachment = Context.Message.Attachments.FirstOrDefault()
                    Dim song As ISong = Await _musicService.Discord.DownloadAsync(
                        attachment.ProxyUrl, $"{Context.Message.Author.Username}#{Context.Message.Author.DiscriminatorValue}",
                        attachment.Id)
                    
                    Await QueueSongAsync(playerContainer, playlist, song, data, response)
                Catch ex As Exception
                    response.ModifyAsync(Sub(x) x.Content = 
                        $"An error has occured! {Format.Bold("ERROR : ") + ex.Message}").GetAwaiter()
                    
                    Dim output As New StringBuilder
                    
                    output.AppendFormat("<{0} ({1})> Upload From Discord Failure!", Context.Guild.Name, 
                                        Context.Guild.Id).AppendLine() _
                        .AppendLine(ex.ToString())
                    
                    _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicManager).GetAwaiter()
                End Try
            End Function
            
            Private Async Function AddByIdAsync(input As String, playerContainer As PlayerContainer, playlist As Playlist, 
                                                data As ServerMusic, response As IUserMessage) As Task
                Dim song As ISong = Nothing
                
                If Await _musicService.TryGetSongAsync(SongId.Parse(input), Sub(songOut) song = songOut)
                    Await QueueSongAsync(playerContainer, playlist, song, data, response)
                    Return
                ElseIf Not (input.Contains("YOUTUBE#"))
                    Await response.ModifyAsync(Sub(x) x.Content = "Specified song does not exist.")
                    Return
                End If
                
                Dim ytUrl = $"https://youtu.be/{input.Split("#"c, 2).LastOrDefault()}"
                Await AddByUrlAsync(ytUrl, playerContainer, playlist, data, response)
            End Function
            
            Private Async Function AddByUrlAsync(input As String, playerContainer As PlayerContainer, playlist As Playlist, 
                                                 data As ServerMusic, response As IUserMessage) As Task
                Dim videoId As String = String.Empty
                Dim song As ISong = Nothing
                
                If Not (input.Contains("youtu"))
                    Await response.ModifyAsync(Sub(x) x.Content = "Only YouTube links can be processed.")
                    Return
                ElseIf Not (_musicService.Youtube.TryParseYoutubeUrl(input, videoId))
                    Await response.ModifyAsync(Sub(x) x.Content = "Invalid Youtube Video Link")
                    Return
                ElseIf Await _musicService.TryGetSongAsync(New SongId("YOUTUBE", videoId), Sub(songOut) song = songOut)
                    If playlist.Songs.Contains(song.Id)
                        Await QueueSongAsync(playerContainer, playlist, song, data, response)
                        Return
                    ElseIf Not (data.AutoDownload)
                        Await response.ModifyAsync(
                            Sub(x) x.Content = "Unable to queue song! Auto-Download is disabled!")
                        Return
                    End If
                End If
                
                Try
                    song = Await _musicService.Youtube.DownloadAsync(New Uri(input))
                    Await QueueSongAsync(playerContainer, playlist, song, data, response)
                Catch ex As Exception
                    response.ModifyAsync(Sub(x) _
                        x.Content = $"An error has occured! {Format.Bold("ERROR : ") + ex.Message}").GetAwaiter()
                    
                    Dim output As New StringBuilder
                    
                    output.AppendFormat("<{0} ({1})> Download From Youtube Failure!", Context.Guild.Name, 
                                        Context.Guild.Id).AppendLine().AppendLine(ex.Message)
                    
                    _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicManager).GetAwaiter()
                End Try
            End Function
            
            Private Async Function SearchAsync(input As String, playerContainer As PlayerContainer, playlist As Playlist, 
                                               data As ServerMusic, response As IUserMessage) As Task
                If String.IsNullOrEmpty(_config.GoogleApiToken)
                    Await response.ModifyAsync(Sub(x) x.Content = 
                        "Music Search is disabled due to no YouTube API V3 key being installed. Please contact the master admin.")
                    Return
                ElseIf String.IsNullOrWhiteSpace(input)
                    Await response.ModifyAsync(Sub(x) x.Content = "Search Query can not be empty.")
                    Return
                End If
                
                Dim search As New YoutubeSearch(_config)
                Dim video As YoutubeVideoData = Await search.GetVideoAsync(input)
                
                If video Is Nothing
                    Await response.ModifyAsync(Sub(x) x.Content = "Unable to find anything using that query.")
                    Return
                End If
                
                await AddByUrlAsync($"https://youtu.be/{video.Id}", playerContainer, playlist, data, response)
            End Function
            
        End Class
    
    End Class
    
End NameSpace