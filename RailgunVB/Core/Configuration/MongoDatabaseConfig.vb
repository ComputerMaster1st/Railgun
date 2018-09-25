Namespace Core.Configuration
    
    Public Class MongoDatabaseConfig
    
        Public ReadOnly Property Hostname As String
        Public ReadOnly Property Username As String
        Public ReadOnly Property Password As String

        Public Sub New(hostname As String, username As String, password As String)
            Me.Hostname = hostname
            Me.Username = username
            Me.Password = password
        End Sub
        
    End Class
    
End NameSpace