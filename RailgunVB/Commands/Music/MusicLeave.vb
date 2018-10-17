Imports Discord.Commands
Imports RailgunVB.Core.Managers

Namespace Commands.Music
        
    Partial Public Class Music
    
        <Group("leave")>
        Public Class MusicLeave
            Inherits ModuleBase
            
            Private ReadOnly _playerManager As PlayerManager
            
            Public Sub New(playerManager As PlayerManager)
                _playerManager = playerManager
            End Sub
            
            <Command>
            Public Async Function LeaveAsync() As Task
                If Not (_playerManager.IsCreated(Context.Guild.Id))
                    await ReplyAsync("I'm not streaming any music at this time.")
                    Return
                End If
            
                await ReplyAsync("Stopping Music Stream...")
                _playerManager.DisconnectPlayer(Context.Guild.Id)
            End Function
        End Class
    
    End Class
    
End NameSpace