Imports AudioChord
Imports Discord
Imports Microsoft.EntityFrameworkCore
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
            Dim sMention As ServerMention = await _context.ServerMentions.FirstOrDefaultAsync(
                Function(find) find.Id = user.GuildId)
            Dim uMention As UserMention = await _context.UserMentions.FirstOrDefaultAsync(
                Function(find) find.Id = user.Id)
            
            If (sMention IsNot Nothing AndAlso sMention.DisableMentions) OrElse 
               (uMention IsNot Nothing AndAlso uMention.DisableMentions)
                Return user.Username
            Else 
                Return user.Mention
            End If
        End Function
        
        Public Async Function GetPlaylistAsync(data As ServerMusic) As Task(Of Playlist)
            If Not (data.PlaylistId = ObjectId.Empty) Then Return Await _musicService.GetPlaylistAsync(data.PlaylistId)

            Dim playlist As Playlist = Await _musicService.CreatePlaylist()
            data.PlaylistId = playlist.Id
            
            Await _context.SaveChangesAsync()
            
            Return playlist
        End Function
        
    End Class
    
End NameSpace