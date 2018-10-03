Imports Newtonsoft.Json

Namespace Core.Configuration
    
    <JsonObject(MemberSerialization.Fields)>
    Public Class DatabaseConfig
    
        Public ReadOnly Property Hostname As String
        Public ReadOnly Property Username As String
        Public ReadOnly Property Password As String
        Public ReadOnly Property Database As String

        <JsonConstructor>
        Public Sub New(hostname As String, username As String, password As String, database As String)
            Me.Hostname = hostname
            Me.Username = username
            Me.Password = password
            Me.Database = database
        End Sub
        
    End Class
    
End NameSpace