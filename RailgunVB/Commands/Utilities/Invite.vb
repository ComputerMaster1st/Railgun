Imports Discord.Commands
Imports RailgunVB.Core

Namespace Commands.Utilities
    
    <Group("invite")>
    Public Class Invite
        Inherits SystemBase
    
        <Command>
        Public Function SendInviteAsync() As Task
            Return ReplyAsync("Railgun Invite Link: https://discordapp.com/api/oauth2/authorize?client_id=261878358625746964&permissions=3271687&scope=bot")
        End Function
        
    End Class
    
End NameSpace