Imports Google.Apis.Services
Imports Google.Apis.YouTube.v3
Imports Google.Apis.YouTube.v3.Data
Imports RailgunVB.Core.Configuration

Namespace Core.Api.Youtube
    
    Public Class YoutubeSearch
        
        Private ReadOnly _service As YouTubeService
        
        Public Sub New(config As MasterConfig)
            _service = New YouTubeService(New BaseClientService.Initializer() With {
                                             .ApiKey = config.GoogleApiToken,
                                             .ApplicationName = Me.GetType().ToString()
                                             })
        End Sub
    
        Public Async Function GetVideoAsync(query As String) As Task(Of YoutubeVideoData)
            Dim request As SearchResource.ListRequest = _service.Search.List("snippet")
            
            request.Q= query
            request.MaxResults = 1
            
            Dim response As SearchListResponse = Await request.ExecuteAsync()
            
            If response.Items.Count < 1 Then Return Nothing
            
            Dim result As SearchResult = response.Items.FirstOrDefault()
            
            If result Is Nothing OrElse Not (result.Id.Kind = "youtube#video") Then Return Nothing
            
            Dim snippet As SearchResultSnippet = result.Snippet
            
            Return New YoutubeVideoData(snippet.Title, result.Id.VideoId, snippet.ChannelTitle)
        End Function
        
    End Class
    
End NameSpace