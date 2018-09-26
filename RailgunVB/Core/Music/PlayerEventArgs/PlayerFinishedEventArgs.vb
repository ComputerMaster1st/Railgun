Namespace Core.Music.PlayerEventArgs
    
    Public Class PlayerFinishedEventArgs
        Inherits PlayerEventArgs
        
        Public ReadOnly Property AutoDisconnected As Boolean
        Public ReadOnly Property Exception As Exception
        
        Public Sub New(guildId As ULong, autoDisconnected As Boolean, Optional exception As Exception = Nothing)
            MyBase.New(guildId)
            Me.AutoDisconnected = autoDisconnected
            Me.Exception = exception
        End Sub
        
    End Class
    
End NameSpace