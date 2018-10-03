Imports System.Text
Imports AudioChord
Imports Discord
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Core.Managers
    
    Public Class MusicManager
        
        Private ReadOnly _log As Log
        Private ReadOnly _commandUtils As CommandUtils
        
        Private ReadOnly _dbContext As TreeDiagramContext
        Private WithEvents _musicService As MusicService
        
        Public Sub New(log As Log, commandUtils As CommandUtils, dbContext As TreeDiagramContext, 
                       musicService As MusicService)
            _log = log
            _commandUtils = commandUtils
            _dbContext = dbContext
            _musicService = musicService
        End Sub
        
        Public Async Function AddYoutubeSongsAsync(guildId As ULong, urlList As List(Of String), 
                                                   tc As ITextChannel) As Task
            Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(guildId)
            Dim playlist As Playlist = Await _commandUtils.GetPlaylistAsync(data)
            Dim response As IUserMessage
            Dim playlistModified = False
                    
            For Each url As String In urlList
                response = Await tc.SendMessageAsync($"{Format.Bold("Processing :")} <{url}>...")
                
                Dim cleanUrl As String = url.Trim(" "c, "<"c, ">"c)
                Dim videoId As String = String.Empty
                Dim song As ISong = Nothing
                        
                If Not (_musicService.Youtube.TryParseYoutubeUrl(url, videoId))
                    Await response.ModifyAsync(
                        Sub(properties) properties.Content = $"{Format.Bold("Invalid Url :")} <{cleanUrl}>")
                    Continue For
                ElseIf Await _musicService.TryGetSongAsync(New SongId("YOUTUBE", videoId), Sub(out) song = out)
                    If playlist.Songs.Contains(song)
                        Await response.ModifyAsync(
                            Sub(properties) properties.Content = 
                                $"{Format.Bold("Already Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}")
                    Else 
                        playlist.Songs.Add(song)
                        playlistModified = True
                        Await response.ModifyAsync(
                            Sub(properties) properties.Content = 
                                $"{Format.Bold("Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}")
                    End If
                            
                    Continue For
                End If
                        
                Try
                    song = Await _musicService.Youtube.DownloadAsync(New Uri(url))
                    playlist.Songs.Add(song)
                    playlistModified = True
                    Await response.ModifyAsync(
                        Sub(properties) properties.Content = 
                            $"{Format.Bold("Encoded & Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}")
                Catch ex As Exception
                    response.ModifyAsync(
                        Sub(properties) properties.Content = 
                            $"{Format.Bold("Failed To Install :")} (<{cleanUrl}>), {ex.Message}").GetAwaiter()
                End Try
            Next
            
            If playlistModified Then Await _musicService.Playlist.UpdateAsync(playlist)
            Await _dbContext.SaveChangesAsync()
            Await tc.SendMessageAsync("Done!")
        End Function
        
        Public Async Function YoutubePlaylistStatusUpdatedAsync(tc As ITextChannel, status As SongProcessStatus, 
                                                                data As ServerMusic) As Task
            Select status.Status
                Case SongStatus.Errored
                    Dim output As SongProcessError = status
                    Dim url As String = "https://youtu.be/" + output.Id.SourceId
                    
                    Await tc.SendMessageAsync(
                        $"{Format.Bold("Failed To Install :")} (<{url}>), {output.Exceptions.Message}")
                    
                    Dim logOutput As New StringBuilder
                    
                    logOutput.AppendFormat("<{0} (1)> Process Failure!", tc.Guild.Name, tc.GuildId).AppendLine() _ 
                        .AppendFormat("{0} - {1}", url, output.Exceptions.Message)
                    
                    Await _log.LogToBotLogAsync(logOutput.ToString(), BotLogType.AudioChord)
                    Exit Select
                Case SongStatus.Processed
                    Dim output As SongProcessResult = status
                    Dim song As ISong = Await output.Result
                    Dim playlist As Playlist = Await _commandUtils.GetPlaylistAsync(data)
                    
                    playlist.Songs.Add(song)
                    Await _musicService.Playlist.UpdateAsync(playlist)
                    Await tc.SendMessageAsync(
                        $"{Format.Bold("Encoded & Installed :")} ({song.Id.ToString()}) {song.Metadata.Name}")
                    
                    Dim logOutput As New StringBuilder
                    
                    logOutput.AppendFormat("<{0} (1)> Processed Song!", tc.Guild.Name, tc.GuildId).AppendLine() _ 
                        .AppendFormat("{0} <{1}> - {2}", song.Id.ToString(), song.Metadata.Length.ToString(), 
                                      song.Metadata.Name)
                    
                    Await _log.LogToBotLogAsync(logOutput.ToString(), BotLogType.AudioChord)
                    Exit Select
            End Select
            
        End Function
        
    End Class
    
End NameSpace