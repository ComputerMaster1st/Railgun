Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Utilities

Namespace Commands
    
    <Group("hello")>
    Public Class Hello
        Inherits ModuleBase
        
        Private ReadOnly _commandUtils As CommandUtils

        Public Sub New(commandUtils As CommandUtils)
            _commandUtils = commandUtils
        End Sub
        
        <Command>
        Public Async Function HelloAsync() As Task
            Dim name As String = Await _commandUtils.GetUsernameOrMentionAsync(Context.User)
            await ReplyAsync($"Hello {Format.Bold(name)}, I'm Railgun! Here to shock your world!")
        End Function
        
    End Class
    
End NameSpace