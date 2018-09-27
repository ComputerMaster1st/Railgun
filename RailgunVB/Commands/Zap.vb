Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Utilities

Namespace Commands
    
    <Group("zap")>
    Public Class Zap
        Inherits ModuleBase
        
        Private ReadOnly _client As IDiscordClient
        Private ReadOnly _commandUtils As CommandUtils

        Public Sub New(client As IDiscordClient, commandUtils As CommandUtils)
            _client = client
            _commandUtils = commandUtils
        End Sub
        
        <Command>
        Public Async Function ZapAsync(Optional user As IGuildUser = Nothing) As Task
            If user IsNot Nothing AndAlso user.Id = _client.CurrentUser.Id
                await ReplyAsync("I'm immune to electricity, BAKA!")
                Return
            End If
            
            Dim name As String = Await _commandUtils.GetUsernameOrMentionAsync(If(user, Context.User))
            await ReplyAsync($"{Format.Bold(name)} has been electrocuted! Something smells nice doesn't it?")
        End Function
        
    End Class
    
End NameSpace