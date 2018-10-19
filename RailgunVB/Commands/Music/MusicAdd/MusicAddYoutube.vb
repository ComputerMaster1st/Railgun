Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Preconditions
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    
    Partial Public Class Music
        
        Partial Public Class MusicAdd
            
            <Group("youtube")>
            Public Class MusicAddYoutube
                Inherits ModuleBase
            
                Private ReadOnly _commandUtils As CommandUtils
                Private ReadOnly _musicManager As MusicManager
                Private ReadOnly _dbContext As TreeDiagramContext
                Private ReadOnly _musicService As MusicService

                Public Sub New(commandUtils As CommandUtils, musicManager As MusicManager, 
                               dbContext As TreeDiagramContext, musicService As MusicService)
                    _commandUtils = commandUtils
                    _musicManager = musicManager
                    _dbContext = dbContext
                    _musicService = musicService
                End Sub
                
                <Command("video")>
                Public Async Function AddVideoAsync(<Remainder> urls As String) As Task
                    Dim urlList As List(Of String) = (From url In urls.Split(","c, " "c) _ 
                            Where Not (String.IsNullOrWhiteSpace(url)) Select url.Trim(" "c, "<"c, ">"c)).ToList()
                    
                    Await Task.Run(New Action(
                        Async Sub() Await _musicManager.AddYoutubeSongsAsync(
                            Context.Guild.Id, urlList, Context.Channel)))
                End Function
                
                <Command("playlist"), UserPerms(GuildPermission.ManageGuild)>
                Public Async Function AddPlaylistAsync(url As String) As Task
                    Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                    DIm playlist As Playlist = Await _commandUtils.GetPlaylistAsync(data)
                    Dim reporter As New Progress(Of SongProcessStatus)(
                        Async Sub(status) Await _musicManager.YoutubePlaylistStatusUpdatedAsync(
                            Context.Channel, status, data))
                    Dim resolvingPlaylist As ResolvingPlaylist = 
                        Await _musicService.Youtube.DownloadPlaylistAsync(
                            New Uri(url.Trim(" "c, "<"c, ">"c)), reporter)

                    Dim output As New StringBuilder
                    Dim queued As Integer = resolvingPlaylist.Songs.Count - resolvingPlaylist.ExistingSongs
                    
                    output.AppendFormat(
                        "Importing From Repository : {0}",
                        Format.Bold(resolvingPlaylist.ExistingSongs))
                    
                    If queued > 0
                        output.AppendFormat(" || Queued For Installation : {0}", Format.Bold(queued)).AppendLine() _
                            .AppendLine("Processing of queued songs may take some time... Just to let you know.")
                    End If
                    
                    Await Task.Run(New Action(
                        Async Sub() Await _musicManager.ProcessYoutubePlaylistAsync(playlist, resolvingPlaylist)))
                    Await ReplyAsync(output.ToString())
                End Function

            End Class
            
        End Class
        
    End Class
    
End NameSpace