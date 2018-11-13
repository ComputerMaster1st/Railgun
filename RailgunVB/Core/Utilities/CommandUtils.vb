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
                Dim db As TreeDiagramContext = scope.ServiceProvider.GetService(Of TreeDiagramContext)
                sMention = await db.ServerMentions.GetAsync(user.GuildId)
                uMention = await db.UserMentions.GetAsync(user.Id)
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
            
            Return playlist
        End Function
        
        Public Async Function CheckIfSelfIsHigherRole(guild As IGuild, user As IGuildUser) As Task(Of Boolean)
            Dim selfRolePosition = 0
            Dim userRolePosition = 0
            Dim self As IGuildUser = Await guild.GetCurrentUserAsync()
            
            For Each roleId As ULong In self.RoleIds
                Dim role As IRole = Guild.GetRole(roleId)
                If role.Permissions.BanMembers AndAlso 
                   role.Position > selfRolePosition Then selfRolePosition = role.Position
            Next
            
            For Each roleId As ULong In user.RoleIds
                Dim role As IRole = Guild.GetRole(roleId)
                If role.Position > userRolePosition Then userRolePosition = role.Position
            Next
            
            If selfRolePosition > userRolePosition Then Return True
            Return False
        End Function
        
    End Class
    
End NameSpace