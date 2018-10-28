Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports MongoDB.Bson
Imports RailgunVB.Core
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music

    Partial Public Class Music
    
        <Group("remove"), UserPerms(GuildPermission.ManageGuild)>
        Public Class MusicRemove
            Inherits SystemBase
            
            Private ReadOnly _playerManager As PlayerManager
            Private ReadOnly _musicService As MusicService

            Public Sub New(playerManager As PlayerManager, musicService As MusicService)
                _playerManager = playerManager
                _musicService = musicService
            End Sub
            
            <Command, Priority(0)>
            Public Async Function RemoveAsync(<Remainder> ids As String) As Task
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetAsync(Context.Guild.Id)
                
                If data Is Nothing OrElse data.PlaylistId = ObjectId.Empty
                    await ReplyAsync("Unknown Music Id Given!")
                    Return
                End If
                
                Dim playlist As Playlist = Await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId)
                Dim output As New StringBuilder
                
                For Each id As String In ids.Split(","c, " "c)
                    If Not (id.Contains("#"c)) Then Continue For
                    
                    Dim song As ISong = Nothing
                    
                    If Not (Await _musicService.TryGetSongAsync(SongId.Parse(id), Sub(songOutput) song = songOutput))
                        output.AppendFormat("{0} - Unknown Music Id Given!", id).AppendLine()
                        Continue For
                    ElseIf Not (playlist.Songs.Contains(song.Id))
                        output.AppendFormat("{0} - Unknown Music Id Given!", id).AppendLine()
                        Continue For
                    End If
                
                    playlist.Songs.Remove(song.Id)
                    output.AppendFormat("{0} - Song Removed", id)
                Next
                
                Await _musicService.Playlist.UpdateAsync(playlist)
                await ReplyAsync(output.ToString())
            End Function
            
            <Command("current"), Priority(1)>
            Public Async Function CurrentAsync() As Task
                Dim playerContainer As PlayerContainer = _playerManager.GetPlayer(Context.Guild.Id)
                
                If playerContainer Is Nothing
                    Await ReplyAsync("Cannot use this command if there's no active music stream at this time.")
                    Return
                End If
                
                Dim player As Player = playerContainer.Player
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetAsync(Context.Guild.Id)
                
                If data Is Nothing OrElse data.PlaylistId = ObjectId.Empty OrElse player Is Nothing
                    Await ReplyAsync("Can not remove current song because I am not in voice channel.")
                    Return
                End If
                
                Dim playlist As Playlist = Await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId)
                
                playlist.Songs.Remove(player.GetFirstSongRequest().Id)
                
                Await _musicService.Playlist.UpdateAsync(playlist)
                await ReplyAsync("Removed from playlist. Skipping to next song...")
                
                player.CancelMusic()
            End Function
            
        End Class
        
    End Class
    
End NameSpace