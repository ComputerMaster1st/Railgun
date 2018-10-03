Imports System.IO
Imports System.Net.Http
Imports RailgunVB.Core.Configuration

Namespace Core.Api
    
    Public Class RandomCat
    
        Private ReadOnly _client As New HttpClient
        Private Const BaseUrl As String = "http://thecatapi.com/api/images/"
        Private ReadOnly _apiKey As String
        
        Public Sub New(config As MasterConfig)
            _apiKey = config.RandomCatApiToken
        End Sub
        
        Public Async Function GetRandomCatAsync() As Task(Of Stream)
            Return Await _client.GetStreamAsync($"{BaseUrl}get?api_key={_apiKey}&type=png&size=med")
        End Function
        
    End Class
    
End NameSpace