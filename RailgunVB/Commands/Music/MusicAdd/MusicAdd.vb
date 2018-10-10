Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Preconditions
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    Partial Public Class Music
        
        <Group("add")>
        Partial Public Class MusicAdd
            Inherits ModuleBase
            
            Private ReadOnly _log As Log
            Private ReadOnly _commandUtils As CommandUtils
            Private ReadOnly _dbContext As TreeDiagramContext
            Private ReadOnly _musicService As MusicService

            Public Sub New(log As Log, commandUtils As CommandUtils, dbContext As TreeDiagramContext, 
                           musicService As MusicService)
                _log = log
                _commandUtils = commandUtils
                _dbContext = dbContext
                _musicService = musicService
            End Sub
            
            <Command("upload")>
            Public Async Function UploadAsync() As Task
                If Context.Message.Attachments.Count < 1
                    await ReplyAsync("Please specify a youtube link or upload a file.")
                    Return
                End If
                
                Dim response As IUserMessage = await ReplyAsync("Processing Attachment! Standby...")
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                Dim playlist As Playlist = await _commandUtils.GetPlaylistAsync(data)
                Dim attachment As IAttachment = Context.Message.Attachments.FirstOrDefault()
                
                Try
                    Dim song As ISong = await _musicService.Discord.DownloadAsync(attachment.ProxyUrl, 
                        $"{Context.User.Username}#{Context.User.DiscriminatorValue}", attachment.Id)
                    
                    playlist.Songs.Add(song.Id)
                    
                    Await _musicService.Playlist.UpdateAsync(playlist)
                    Await response.ModifyAsync(Function(c) c.Content = 
                        $"Installed To Playlist - {Format.Bold(song.Metadata.Name)} || ID : {Format.Bold(song.Id.ToString())}")
                Catch ex As Exception
                    response.ModifyAsync(Function(c) c.Content = 
                        $"Install Failure - {Format.Bold("(Attached File)")} {ex.Message}").GetAwaiter()
                    
                    Dim output As New StringBuilder
                    
                    output.AppendFormat("<{0} ({1})> Upload From Discord Failure!", Context.Guild.Name, 
                                        Context.Guild.Id).AppendLine() _ 
                        .AppendLine(ex.ToString())
                    
                    _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicManager).GetAwaiter()
                End Try
            End Function
            
            <Command("repo"), UserPerms(GuildPermission.ManageGuild)>
            Public Async Function ImportRepoAsync() As Task
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                Dim repo = (Await _musicService.GetAllSongsAsync()).ToList()
                Dim playlist As Playlist = Await _commandUtils.GetPlaylistAsync(data)
                Dim existingSongs = 0
                
                For Each song As ISong in repo
                    If playlist.Songs.Contains(song.Id)
                        existingSongs += 1
                    Else
                        playlist.Songs.Add(song.Id)
                    End If
                Next
                
                Await _musicService.Playlist.UpdateAsync(playlist)
                await ReplyAsync($"Processing Completed! || Accepted : {Format.Bold(
                    (repo.Count() - existingSongs).ToString())} || Already Installed : {Format.Bold(
                        existingSongs.ToString())}")
            End Function
        End Class
        
    End Class
    
End NameSpace