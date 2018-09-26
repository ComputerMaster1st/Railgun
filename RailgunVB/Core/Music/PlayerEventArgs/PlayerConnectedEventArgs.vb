Namespace Core.Music.PlayerEventArgs
    
    Public Class PlayerConnectedEventArgs
        Inherits PlayerEventArgs

        Public Sub New(guildId As ULong)
            MyBase.New(guildId)
        End Sub
        
    End Class
    
End NameSpace