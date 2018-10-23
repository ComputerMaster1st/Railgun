Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music

Namespace Commands.Music
        
    Partial Public Class Music
    
        <Group("leave")>
        Public Class MusicLeave
            Inherits SystemBase
            
            Private ReadOnly _playerManager As PlayerManager
            
            Public Sub New(playerManager As PlayerManager)
                _playerManager = playerManager
            End Sub
            
            <Command>
            Public Async Function LeaveAsync() As Task
                If Not (_playerManager.IsCreated(Context.Guild.Id))
                    Await ReplyAsync("I'm not streaming any music at this time.")
                    Return
                End If
            
                await ReplyAsync("Stopping Music Stream...")
                _playerManager.DisconnectPlayer(Context.Guild.Id)
            End Function
            
            <Command("aftersong")>
            Public Async Function LeaveAfterSongAsync() As Task
                Dim container As PlayerContainer = _playerManager.GetPlayer(Context.Guild.Id)
                
                If container Is Nothing
                    Await ReplyAsync("I'm not streaming any music at this time.")
                    Return
                End If
                
                container.Player.LeaveAfterSong = True
                Await ReplyAsync("I shall leave after this song has finished playing.")
            End Function
            
        End Class
    
    End Class
    
End NameSpace