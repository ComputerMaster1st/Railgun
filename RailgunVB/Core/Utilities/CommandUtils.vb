Imports AudioChord
Imports Discord
Imports Microsoft.Extensions.DependencyInjection
Imports MongoDB.Bson
Imports TreeDiagram
Imports TreeDiagram.Models.Server
Imports TreeDiagram.Models.User

Namespace Core.Utilities
    
    Public Class CommandUtils
    
        Private ReadOnly _services As IServiceProvider
        Private ReadOnly _musicService As MusicService

        Public Sub New(services As IServiceProvider)
            _musicService = services.GetService(Of MusicService)
            _services = services
        End Sub
        
        Public Async Function GetUsernameOrMentionAsync(user As IGuildUser) As Task(Of String)
            Dim sMention As ServerMention
            Dim uMention As UserMention
            
            Using scope As IServiceScope = _services.CreateScope()
                Dim context As TreeDiagramContext = scope.ServiceProvider.GetService(Of TreeDiagramContext)
                
                sMention = await context.ServerMentions.GetAsync(user.GuildId)
                uMention = await context.UserMentions.GetAsync(user.Id)
            End Using
            
            If (sMention IsNot Nothing AndAlso sMention.DisableMentions) OrElse 
               (uMention IsNot Nothing AndAlso uMention.DisableMentions)
                Return user.Username
            Else 
                Return user.Mention
            End If
        End Function
        
        Public Async Function GetPlaylistAsync(data As ServerMusic) As Task(Of Playlist)
            If Not (data.PlaylistId = ObjectId.Empty) Then Return Await _musicService.Playlist.GetPlaylistAsync(
                data.PlaylistId)

            Dim playlist As New Playlist
            
            data.PlaylistId = playlist.Id
            
            Await _musicService.Playlist.UpdateAsync(playlist)
            
            Using scope As IServiceScope = _services.CreateScope()
                Await scope.ServiceProvider.GetService(Of TreeDiagramContext).SaveChangesAsync()
            End Using
            
            Return playlist
        End Function
        
    End Class
    
End NameSpace