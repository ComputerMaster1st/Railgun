Namespace Core.Api.Youtube
    
    Public Class YoutubeVideoData
    
        Public ReadOnly Property Name As String
        Public ReadOnly Property Id As String
        Public ReadOnly Property Uploader As String
        
        Friend Sub New(name As String, id As String, uploader As String)
            Me.Name = name
            Me.Id = id
            Me.Uploader = uploader
        End Sub
        
    End Class
    
End NameSpace