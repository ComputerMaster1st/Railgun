Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports MongoDB.Bson
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    Partial Public Class Music
        
        <Group("reset"), UserPerms(GuildPermission.ManageGuild)>
        Public Class MusicReset
            Inherits ModuleBase
            
            Private ReadOnly _config As MasterConfig
            Private ReadOnly _playerManager As PlayerManager
            Private ReadOnly _dbContext As TreeDiagramContext
            Private ReadOnly _musicService AS MusicService
            Private _full As Boolean = False

            Public Sub New(config As MasterConfig, playerManager As PlayerManager, dbContext As TreeDiagramContext, 
                           musicService As MusicService)
                _config = config
                _playerManager = playerManager
                _dbContext = dbContext
                _musicService = musicService
            End Sub
            
            <Command("playlist")>
            Public Async Function PlaylistAsync() As Task
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetAsync(Context.Guild.Id)
                
                If data Is Nothing OrElse data.PlaylistId = ObjectId.Empty AndAlso Not (_full)
                    await ReplyAsync("Server playlist is already empty.")
                    Return
                End If
                
                Await StreamAsync()
                Await _musicService.DeletePlaylistAsync(data.PlaylistId)
                
                data.PlaylistId = ObjectId.Empty
                
                If Not (_full)
                    Await _dbContext.SaveChangesAsync()
                    await ReplyAsync("Server playlist is now empty.")
                End If
            End Function
            
            <Command("stream")>
            Public Async Function StreamAsync() As Task
                If Not (_playerManager.IsCreated(Context.Guild.Id))
                    If Not (_full) Then await ReplyAsync("I'm not streaming any music at this time.")
                    Return
                End If
                
                _playerManager.DisconnectPlayer(Context.Guild.Id)
                
                If Not (_full) Then await ReplyAsync($"Music stream has been reset! Use {Format.Code(
                    $"{_config.DiscordConfig.Prefix}music join")} to create a new music stream.")
            End Function
            
            <Command("full")>
            Public Async Function FullAsync() As Task
                _full = True
                
                Await PlaylistAsync()
                
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetAsync(Context.Guild.Id)
                
                If data Is Nothing
                    await ReplyAsync("Music has no data to reset.")
                    Return
                End If
                
                _dbContext.ServerMusics.Remove(data)
                Await _dbContext.SaveChangesAsync()
                await ReplyAsync("Music settings & playlist has been reset.")
            End Function
            
        End Class
        
    End Class
    
End NameSpace