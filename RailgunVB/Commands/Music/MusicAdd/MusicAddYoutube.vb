Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music.MusicAdd
    
    Partial Public Class Music
        
        Partial Public Class MusicAdd
            
            <Group("youtube")>
            Public Class MusicAddYoutube
                Inherits ModuleBase
            
                Private ReadOnly _commandUtils As CommandUtils
                Private ReadOnly _dbContext As TreeDiagramContext
                Private ReadOnly _musicService As MusicService

                Public Sub New(commandUtils As CommandUtils, dbContext As TreeDiagramContext, 
                               musicService As MusicService)
                    _commandUtils = commandUtils
                    _dbContext = dbContext
                    _musicService = musicService
                End Sub
                
                <Command("video")>
                Public Async Function AddVideoAsync(<Remainder> urls As String) As Task
                    Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                    Dim urlList As List(Of String) = (From url In urls.Split(","c, " "c) _ 
                            Where Not (String.IsNullOrWhiteSpace(url)) Select url.Trim(" "c, "<"c, ">"c)).ToList()
                    
                    await ReplyAsync("Adding songs to processing queue. Standby...")
                    await _musicService.ProcessYTPlaylistAsync(urlList, Context.Guild.Id, Context.Channel.Id, 
                                                               await _commandUtils.GetPlaylistAsync(data))
                End Function
                
                <Command("playlist")>
                Public Async Function AddPlaylistAsync(url As String) As Task
                    Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                    Dim playlistUrl As String = url.Trim(" "c, "<"c, ">"c)
                    Dim response As IUserMessage = await ReplyAsync("Adding songs to processing queue. Standby...")
                    
                    If Not (Await _musicService.ProcessYTPlaylistAsync(playlistUrl, Context.Guild.Id, 
                        Context.Channel.Id, Await _commandUtils.GetPlaylistAsync(data))) Then _ 
                        Await response.ModifyAsync(Function(x) x.Content = 
                            "The YouTube Playlist Url provided failed to parse. Please double check the Url.")
                End Function
            End Class
            
        End Class
        
    End Class
    
End NameSpace