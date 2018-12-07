using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Enums;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    [Alias("music", "m"), RoleLock(ModuleType.Music)]
    public partial class Music : SystemBase
    {
        private readonly MasterConfig _config;
        private readonly PlayerManager _playerManager;
        private readonly MusicService _musicService;

        public Music(MasterConfig config, PlayerManager playerManager, MusicService musicService) {
            _config = config;
            _playerManager = playerManager;
            _musicService = musicService;
        }
        
        [Command("join"), BotPerms(GuildPermission.Connect | GuildPermission.Speak)]
        public async Task JoinAsync() {
            if (_playerManager.IsCreated(Context.Guild.Id)) {
                await ReplyAsync($"Sorry, I'm already in a voice channel. If you're experiencing problems, please do {Format.Code($"{_config.DiscordConfig.Prefix}music reset stream.")}");

                return;
            }
            
            var user = (IGuildUser)Context.Author;
            var vc = user.VoiceChannel;
            
            if (vc == null) {
                await ReplyAsync("Please go into a voice channel before inviting me.");

                return;
            }
            
            await _playerManager.CreatePlayerAsync(user, vc, (ITextChannel)Context.Channel);
        }
        
        [Command("playlist"), BotPerms(ChannelPermission.AttachFiles)]
        public async Task PlaylistAsync() {
            var data = await Context.Database.ServerMusics.GetAsync(Context.Guild.Id);
            
            if (data == null || data.PlaylistId == ObjectId.Empty) {
                await ReplyAsync("Server playlist == currently empty.");

                return;
            }
            
            var playlist = await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId);
            
            if (playlist == null || playlist.Songs.Count < 1) {
                await ReplyAsync("Server playlist == currently empty.");

                return;
            }
            
            var response = await ReplyAsync("Generating playlist file, standby...");
            var removedSongs = new List<SongId>();
            var output = new StringBuilder()
                .AppendFormat("{0} Music Playlist!", Context.Guild.Name).AppendLine()
                .AppendFormat("Total Songs : {0}", playlist.Songs.Count).AppendLine()
                .AppendLine();
            
            foreach (var songId in playlist.Songs) {
                ISong song = null;

                if (!await _musicService.TryGetSongAsync(songId, result => song = result)) {
                    removedSongs.Add(songId);
                    continue;
                }
                
                output.AppendFormat("--       Id => {0}", song.Id.ToString()).AppendLine()
                    .AppendFormat("--     Name => {0}", song.Metadata.Name).AppendLine()
                    .AppendFormat("--   Length => {0}", song.Metadata.Length).AppendLine()
                    .AppendFormat("--      Url => {0}", song.Metadata.Url).AppendLine()
                    .AppendFormat("-- Uploader => {0}", song.Metadata.Uploader).AppendLine()
                    .AppendLine();
            }
            
            output.AppendLine("End of Playlist.");
            
            if (removedSongs.Count > 0) {
                foreach (var songId in removedSongs) playlist.Songs.Remove(songId);
                
                await _musicService.Playlist.UpdateAsync(playlist);
            }
            
            await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "Playlist.txt", output.ToString(), $"{Context.Guild.Name} Music Playlist ({playlist.Songs.Count} songs)");
            await response.DeleteAsync();
        }
        
        [Command("repeat")]
        public async Task RepeatAsync(int count = 1) {
            var container = _playerManager.GetPlayer(Context.Guild.Id);
            
            if (container == null) {
                await ReplyAsync("I'm not playing anything at this time.");

                return;
            }
            
            var player = container.Player;
            
            player.RepeatSong = count;

            await ReplyAsync("Repeating song after finishing.");
        }
        
        [Command("repo"), BotPerms(ChannelPermission.AttachFiles)]
        public async Task RepositoryAsync() {
            var response = await ReplyAsync("Generating repository list, standby...");
            var repo = (await _musicService.GetAllSongsAsync()).ToList();
            var output = new StringBuilder()
                .AppendLine("Railgun Music Repository!")
                .AppendFormat("Total Songs : {0}", repo.Count()).AppendLine()
                .AppendLine();
            
            foreach (var song in repo)
                output.AppendFormat("--       Id => {0}", song.Id.ToString()).AppendLine()
                    .AppendFormat("--     Name => {0}", song.Metadata.Name).AppendLine()
                    .AppendFormat("--   Length => {0}", song.Metadata.Length).AppendLine()
                    .AppendFormat("--      Url => {0}", song.Metadata.Url).AppendLine()
                    .AppendFormat("-- Uploader => {0}", song.Metadata.Uploader).AppendLine()
                    .AppendLine();
            
            output.AppendLine("End of Repository.");
            
            await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "MusicRepo.txt", output.ToString(), $"Music Repository ({repo.Count()} songs)", includeGuildName:false);
            await response.DeleteAsync();
        }
        
        [Command("ping")]
        public Task PingAsync() {
            var container = _playerManager.GetPlayer(Context.Guild.Id);
            
            return ReplyAsync(container == null ? "Can not check ping due to not being in voice channel." : $"Ping to Discord Voice: {Format.Bold(container.Player.Latency.ToString())}ms");
        }
        
        [Command("queue"), BotPerms(ChannelPermission.AttachFiles)]
        public async Task QueueAsync() {
            var playerContainer = _playerManager.GetPlayer(Context.Guild.Id);
            
            if (playerContainer == null) {
                await ReplyAsync("I'm not playing anything at this time.");

                return;
            }

            var player = playerContainer.Player;
            
            if (!player.AutoSkipped && player.Requests.Count < 2) {
                await ReplyAsync("There are currently no music requests in the queue.");

                return;
            }
            
            var i = 0;
            var output = new StringBuilder()
                .AppendFormat(Format.Bold("Queued Music Requests ({0}) :"), player.Requests.Count).AppendLine()
                .AppendLine();
            
            while (player.Requests.Count > i) {
                var song = player.Requests[i];
                var meta = song.Metadata;
                
                switch (i) {
                    case 0:
                        var currentTime = DateTime.Now - player.SongStartedAt;
                        
                        output.AppendFormat("Now : {0} || Length : {1}/{2}", 
                                            Format.Bold(meta.Name),
                                            Format.Bold($"{currentTime.Minutes}:{currentTime.Seconds}"),
                                            Format.Bold($"{meta.Length.Minutes}:{meta.Length.Seconds}"))
                            .AppendLine();
                        break;
                    case 1:
                        output.AppendFormat("Next : {0} || Length : {1}", 
                                            Format.Bold(meta.Name), 
                                            Format.Bold(meta.Length.ToString()));
                        break;
                    default:
                        output.AppendFormat("{0} : {1} || Length : {2}", 
                                            Format.Code($"[{i}]"), 
                                            Format.Bold(meta.Name), 
                                            Format.Bold(meta.Length.ToString()));
                        break;
                }
                
                output.AppendLine();
                i++;
            }
            
            if (output.Length > 1950) {
                await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "Queue.txt", output.ToString(), $"Queued Music Requests ({player.Requests.Count})");

                return;
            }
            
            await ReplyAsync(output.ToString());
        }
        
        [Command("show")]
        public async Task ShowAsync() {
            var data = await Context.Database.ServerMusics.GetAsync(Context.Guild.Id);
            var songCount = 0;
            
            if (data == null) {
                await ReplyAsync("There are no settings available for Music.");

                return;
            } else if (data.PlaylistId != ObjectId.Empty) {
                var playlist = await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId);

                songCount = playlist.Songs.Count;
            }
            
            var vc = data.AutoVoiceChannel != 0 ? await Context.Guild.GetVoiceChannelAsync(data.AutoVoiceChannel) : null;
            var tc = data.AutoTextChannel != 0 ? await Context.Guild.GetTextChannelAsync(data.AutoTextChannel) : null;
            var npTc = data.NowPlayingChannel != 0 ? await Context.Guild.GetTextChannelAsync(data.NowPlayingChannel) : null;
            var autoJoinOutput = string.Format("{0} {1}", vc != null ? vc.Name : "Disabled", tc != null ? $"(#{tc.Name})" : "");
            var autoDownloadOutput = data.AutoDownload ? "Enabled" : "Disabled";
            var autoSkipOutput = data.AutoSkip ? "Enabled" : "Disabled";
            var silentPlayingOutput = data.SilentNowPlaying ? "Enabled" : "Disabled";
            var silentInstallOutput = data.SilentSongProcessing ? "Enabled" : "Disabled";
            var npTcName = npTc != null ? $"#{npTc.Name}" : "None";
            var voteskipOutput = data.VoteSkipEnabled ? $"Enabled @ {data.VoteSkipLimit}% Users" : "Disabled";
            var output = new StringBuilder();
            var roleLock = new StringBuilder();
            
            if (data.AllowedRoles.Count > 0)
                foreach (var allowedRole in data.AllowedRoles) {
                    var role = Context.Guild.GetRole(allowedRole.RoleId);
                    roleLock.AppendFormat("| {0} |", role.Name);
                }
            else roleLock.AppendFormat("None.");
            
            output.AppendLine("Music Settings")
                .AppendLine()
                .AppendFormat("Number Of Songs : {0}", songCount).AppendLine()
                .AppendLine()
                .AppendFormat("      Auto-Join : {0}", autoJoinOutput).AppendLine()
                .AppendFormat("  Auto-Download : {0}", autoDownloadOutput).AppendLine()
                .AppendFormat("      Auto-Skip : {0}", autoSkipOutput).AppendLine()
                .AppendLine()
                .AppendFormat(" Silent Running : {0}", silentPlayingOutput).AppendLine()
                .AppendFormat(" Silent Install : {0}", silentInstallOutput).AppendLine()
                .AppendLine()
                .AppendFormat("NP Dedi Channel : {0}", npTcName).AppendLine()
                .AppendFormat("      Vote-Skip : {0}", voteskipOutput).AppendLine()
                .AppendFormat("    Role-Locked : {0}", roleLock.ToString());
            
            await ReplyAsync(Format.Code(output.ToString()));
        }
    }
}