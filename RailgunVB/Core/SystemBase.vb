Imports Discord.Commands

Namespace Core

    Public Class SystemBase
        Inherits ModuleBase(Of SystemContext)

        Protected Overrides Sub AfterExecute(command As CommandInfo)
            Context.Database = Nothing
            MyBase.AfterExecute(command)
        End Sub
    End Class
End NameSpace