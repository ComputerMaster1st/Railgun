Imports System.IO
Imports System.Text
Imports Discord
Imports Discord.Commands

Namespace Core

    Public Class SystemBase
        Inherits ModuleBase(Of SystemContext)

        Protected Overrides Sub AfterExecute(command As CommandInfo)
            Context.DisposeDatabase()
            MyBase.AfterExecute(command)
        End Sub
        
        Protected Async Function SendStringAsFileAsync(tc As ITextChannel, name As String, output As String, 
                                                    Optional text As String = Nothing, 
                                                    Optional includeGuildName As Boolean = True) As Task
            Await tc.SendFileAsync(New MemoryStream(Encoding.UTF8.GetBytes(output)), 
                                   $"{If(includeGuildName, $"{tc.Guild.Name}-", "") + name}", text)
        End Function
        
    End Class
End NameSpace