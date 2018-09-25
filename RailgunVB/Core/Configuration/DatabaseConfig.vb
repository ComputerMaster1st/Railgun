Namespace Core.Configuration
    
    Public Class DatabaseConfig
    
        Public ReadOnly Hostname As String
        Public ReadOnly Username As String
        Public ReadOnly Password As String
        Public ReadOnly Database As String

        Public Sub New(hostname As String, username As String, password As String, database As String)
            Me.Hostname = hostname
            Me.Username = username
            Me.Password = password
            Me.Database = database
        End Sub
        
    End Class
    
End NameSpace