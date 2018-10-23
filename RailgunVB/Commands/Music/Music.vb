Imports System.IO
Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports MongoDB.Bson
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    
    <Group("music")>
    Partial Public Class Music
        Inherits ModuleBase
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _playerManager As PlayerManager
        Private ReadOnly _dbContext As TreeDiagramContext
        Private ReadOnly _musicService As MusicService

        Public Sub New(config As MasterConfig, playerManager As PlayerManager, dbContext As TreeDiagramContext, 
                       musicService As MusicService)
            _config = config
            _playerManager = playerManager
            _dbContext = dbContext
            _musicService = musicService
        End Sub
        
        <Command("join"), BotPerms(GuildPermission.Connect And GuildPermission.Speak)>
        Public Async Function JoinAsync() As Task
            if _playerManager.IsCreated(Context.Guild.Id)
                await ReplyAsync($"Sorry, I'm already in a voice channel. If you're experiencing problems, please do {Format.Code($"{_config.DiscordConfig.Prefix}music reset stream.")}")
                Return
            End If
            
            Dim user As IGuildUser = Context.User
            Dim vc As IVoiceChannel = user.VoiceChannel
            
            If vc Is Nothing
                await ReplyAsync("Please go into a voice channel before inviting me.")
                Return
            End If
            
            Await _playerManager.CreatePlayerAsync(user, vc, Context.Channel)
        End Function
        
        <Command("playlist"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function PlaylistAsync() As Task
            Dim data As ServerMusic = Await _dbContext.ServerMusics.GetAsync(Context.Guild.Id)
            
            If data Is Nothing OrElse data.PlaylistId = ObjectId.Empty
                await ReplyAsync("Server playlist is currently empty.")
                Return
            End If
            
            Dim playlist As Playlist = Await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId)
            
            If playlist Is Nothing OrElse playlist.Songs.Count < 1
                await ReplyAsync("Server playlist is currently empty.")
                Return
            End If
            
            Dim response = Await ReplyAsync("Generating playlist file, standby...")
            Dim output As new StringBuilder
            Dim removedSongs As New List(Of SongId)

            output.AppendFormat("{0} Music Playlist!", Context.Guild.Name).AppendLine() _ 
                .AppendFormat("Total Songs : {0}", playlist.Songs.Count).AppendLine() _
                .AppendLine()
            
            For Each songId As SongId in playlist.Songs
                Dim song As ISong = Nothing
                If Not (Await _musicService.TryGetSongAsync(songId, Sub(result) song = result))
                    removedSongs.Add(songId)
                    Continue For
                End If
                
                output.AppendFormat("--       Id => {0}", song.Id.ToString()).AppendLine() _
                    .AppendFormat("--     Name => {0}", song.Metadata.Name).AppendLine() _
                    .AppendFormat("--   Length => {0}", song.Metadata.Length).AppendLine() _
                    .AppendFormat("--      Url => {0}", song.Metadata.Url).AppendLine() _
                    .AppendFormat("-- Uploader => {0}", song.Metadata.Uploader).AppendLine() _
                    .AppendLine()
            Next
            
            output.AppendLine("End of Playlist.")
            
            If removedSongs.Count > 0
                For Each songId As SongId In removedSongs
                    playlist.Songs.Remove(songId)
                Next
                
                Await _musicService.Playlist.UpdateAsync(playlist)
            End If
            
            Dim filename As String = ($"{Context.Guild.Name} Playlist.txt").Replace(" "c, "-"c).Trim("/"c)
            
            await File.WriteAllTextAsync(filename, output.ToString())
            await Context.Channel.SendFileAsync(filename, $"{Context.Guild.Name} Music Playlist ({playlist.Songs.Count} songs)")
            Await response.DeleteAsync()
            File.Delete(filename)
        End Function
        
        <Command("repeat")>
        Public Async Function RepeatAsync(Optional count As Integer = 1) As Task
            Dim player As Player = _playerManager.GetPlayer(Context.Guild.Id).Player
            
            If player Is Nothing
                await ReplyAsync("I'm not playing anything at this time.")
                Return
            End If
            
            player.RepeatSong = count
            await ReplyAsync("Repeating song after finishing.")
        End Function
        
        <Command("repo"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function RepositoryAsync() As Task
            Dim response = Await ReplyAsync("Generating repository list, standby...")
            Dim repo = (Await _musicService.GetAllSongsAsync()).ToList()
            Dim output As New StringBuilder
            
            output.AppendLine("Railgun Music Repository!") _
                .AppendFormat("Total Songs : {0}", repo.Count()).AppendLine() _
                .AppendLine()
            
            For Each song As ISong in repo
                output.AppendFormat("--       Id => {0}", song.Id.ToString()).AppendLine() _
                    .AppendFormat("--     Name => {0}", song.Metadata.Name).AppendLine() _
                    .AppendFormat("--   Length => {0}", song.Metadata.Length).AppendLine() _
                    .AppendFormat("--      Url => {0}", song.Metadata.Url).AppendLine() _
                    .AppendFormat("-- Uploader => {0}", song.Metadata.Uploader).AppendLine() _
                    .AppendLine()
            Next
            
            output.AppendLine("End of Repository.")
            
            Const filename = "MusicRepo.txt"
            
            await File.WriteAllTextAsync(filename, output.ToString())
            await Context.Channel.SendFileAsync(filename, $"Music Repository ({repo.Count()} songs)")
            Await response.DeleteAsync()
            File.Delete(filename)
        End Function
        
        <Command("ping")>
        Public Async Function PingAsync() As Task
            Dim player As Player = _playerManager.GetPlayer(Context.Guild.Id).Player
            
            Await ReplyAsync(If(player Is Nothing, "Can not check ping due to not being in voice channel.", 
                                $"Ping to Discord Voice: {Format.Bold(player.Latency.ToString())}ms"))
        End Function
        
        <Command("queue")>
        Public Async Function QueueAsync() As Task
            Dim playerContainer As PlayerContainer = _playerManager.GetPlayer(Context.Guild.Id)
            
            If playerContainer Is Nothing
                await ReplyAsync("I'm not playing anything at this time.")
                Return
            End If

            Dim player As Player = playerContainer.Player
            
            If player.Requests.Count < 2
                await ReplyAsync("There are currently no music requests in the queue.")
                Return
            End If
            
            Dim output As New StringBuilder
            
            output.AppendFormat(Format.Bold("Queued Music Requests ({0}) :"), (player.Requests.Count - 1)).AppendLine() _
                .AppendLine()
            
            Dim i = 1
            While player.Requests.Count > i
                Dim song As ISong = player.Requests(i)
                Dim meta As SongMetadata = song.Metadata
                
                output.AppendFormat("{0} : {1} || Length : {2}", If(i = 1, "Next", Format.Code($"[{i}]")), 
                                    Format.Bold(meta.Name), Format.Bold(meta.Length.ToString())).AppendLine()
                
                i += 1
            End While
            
            await ReplyAsync(output.ToString())
        End Function
        
        <Command("show")>
        Public Async Function ShowAsync() As Task
            Dim data As ServerMusic = Await _dbContext.ServerMusics.GetAsync(Context.Guild.ID)
            Dim songCount = 0
            
            If data Is Nothing
                await ReplyAsync("There are no settings available for Music.")
                Return
            ElseIf data.PlaylistId <> ObjectId.Empty
                Dim playlist As Playlist = Await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId)
                songCount = playlist.Songs.Count
            End If
            
            Dim vc As IVoiceChannel = If(data.AutoVoiceChannel <> 0, 
                Await Context.Guild.GetVoiceChannelAsync(data.AutoVoiceChannel), Nothing)
            Dim tc As ITextChannel = If(data.AutoTextChannel <> 0, 
                Await Context.Guild.GetTextChannelAsync(data.AutoTextChannel), Nothing)
            Dim npTc As ITextChannel = If(data.NowPlayingChannel <> 0, 
                Await Context.Guild.GetTextChannelAsync(data.NowPlayingChannel), Nothing)
            Dim output As New StringBuilder
            
            output.AppendLine("Music Settings") _
                .AppendLine() _
                .AppendFormat("Number Of Songs : {0}", songCount).AppendLine() _
                .AppendLine() _
                .AppendFormat("      Auto-Join : {0} {1}", If(vc IsNot Nothing, vc.Name, "Disabled"), 
                              If(tc IsNot Nothing, $"(#{tc.Name})", "")).AppendLine() _
                .AppendFormat("  Auto-Download : {0}", If(data.AutoDownload, "Enabled", "Disabled")).AppendLine() _
                .AppendFormat("      Auto-Skip : {0}", If(data.AutoSkip, "Enabled", "Disabled")).AppendLine() _
                .AppendLine() _
                .AppendFormat(" Silent Running : {0}", If(data.SilentNowPlaying, "Enabled", "Disabled")).AppendLine() _
                .AppendFormat(" Silent Install : {0}", If(data.SilentSongProcessing, "Enabled", "Disabled")).AppendLine() _
                .AppendLine() _
                .AppendFormat("NP Dedi Channel : {0}", If(npTc IsNot Nothing, $"#{npTc.Name}", "None"))
            
            await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
    End Class
    
End NameSpace