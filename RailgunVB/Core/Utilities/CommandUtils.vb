Imports AudioChord
Imports Discord
Imports MongoDB.Bson
Imports TreeDiagram
Imports TreeDiagram.Models.Server
Imports TreeDiagram.Models.User

Namespace Core.Utilities
    
    Public Class CommandUtils
    
        Private ReadOnly _context As TreeDiagramContext
        Private ReadOnly _musicService As MusicService

        Public Sub New(context As TreeDiagramContext, musicService As MusicService)
            _context = context
            _musicService = musicService
        End Sub
        
        Public Async Function GetUsernameOrMentionAsync(user As IGuildUser) As Task(Of String)
            Dim sMention As ServerMention = await _context.ServerMentions.GetAsync(user.GuildId)
            Dim uMention As UserMention = await _context.UserMentions.GetAsync(user.Id)
            
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
            Await _context.SaveChangesAsync()
            
            Return playlist
        End Function
        
    End Class
    
End NameSpace