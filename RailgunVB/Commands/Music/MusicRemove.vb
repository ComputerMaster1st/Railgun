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
            Public Async Function RemoveAsync(id As String) As Task
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetAsync(Context.Guild.Id)
                
                If data Is Nothing OrElse data.PlaylistId = ObjectId.Empty
                    await ReplyAsync("Unknown Music Id Given!")
                    Return
                End If
                
                Dim playlist As Playlist = Await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId)
                Dim song As ISong = Nothing
                
                If Not (Await _musicService.TryGetSongAsync(SongId.Parse(id), Sub(output) song = output))
                    Await ReplyAsync("Unknown Music Id Given!")
                    Return
                ElseIf Not (playlist.Songs.Contains(song.Id))
                    Await ReplyAsync("Unknown Music Id Given!")
                    Return
                End If
                
                playlist.Songs.Remove(song.Id)
                
                Await _musicService.Playlist.UpdateAsync(playlist)
                await ReplyAsync("Music removed from playlist.")
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