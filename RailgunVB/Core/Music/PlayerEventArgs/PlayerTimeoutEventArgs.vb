Namespace Core.Music.PlayerEventArgs
    
    Public Class PlayerTimeoutEventArgs
        Inherits PlayerEventArgs
        
        Public ReadOnly Property Exception As TimeoutException
        
        Public Sub New(guildId As ULong, exception As TimeoutException)
            MyBase.New(guildId)
            Me.Exception = exception
        End Sub
    End Class
    
End NameSpace