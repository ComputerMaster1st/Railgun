Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Utilities

Namespace Commands.Fun
    
    <Group("roll")>
    Public Class Roll
        Inherits ModuleBase
        
        Private ReadOnly _commandUtils As CommandUtils

        Public Sub New(commandUtils As CommandUtils)
            _commandUtils = commandUtils
        End Sub
        
        <Command>
        Public Async Function RollAsync(Optional num1 As Integer = 0, Optional num2 As Integer = 100) As Task
            Dim rand As New Random
            Dim rng = 0
            
            If num1 > num2
                rng = rand.Next(num2, num1)
            Else 
                rng = rand.Next(num1, num2)
            End If
            
            Dim name As String = await _commandUtils.GetUsernameOrMentionAsync(Context.User)
            await ReplyAsync($"{Format.Bold(name)} has rolled {Format.Bold(rng.ToString())}.")
        End Function
    End Class
    
End NameSpace