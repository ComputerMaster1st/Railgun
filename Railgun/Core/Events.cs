using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Railgun.Core.Configuration;
using Railgun.Core.Logging;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Enums;
using TreeDiagram.Models.Server;

namespace Railgun.Core
{
    public class Events
    {
        private readonly IServiceProvider _services;
        private readonly MasterConfig _config;
        private readonly Log _log;
        private readonly DiscordShardedClient _client;
        private readonly CommandUtils _commandUtils;
        private readonly ServerCount _serverCount;
        private readonly MusicService _musicService;
        private readonly PlayerManager _playerManager;
//         Private ReadOnly _timerManager As TimerManager

        private bool _initialized = false;
        private readonly Dictionary<int, bool> _shardsReady = new Dictionary<int, bool>();

        public Events(IServiceProvider services) {
            _services = services;

            _config = _services.GetService<MasterConfig>();
            _log = _services.GetService<Log>();
            _commandUtils = _services.GetService<CommandUtils>();
            _serverCount = _services.GetService<ServerCount>();
            _client = _services.GetService<DiscordShardedClient>();
            _musicService = _services.GetService<MusicService>();
            _playerManager =  _services.GetService<PlayerManager>();
//             _timerManager = services.GetService(Of TimerManager)

            _client.JoinedGuild += JoinedGuildAsync;
            _client.LeftGuild += LeftGuildAsync;
            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdatedAsync;
            _client.ShardReady += ShardReadyAsync;
        }

        private async Task SendJoinLeaveMessageAsync(ServerJoinLeave data, IGuildUser user, string message) {
            if (string.IsNullOrEmpty(message)) return;
            else if (data.ChannelId == 0) return;
            else if (data.SendToDM) {
                try {
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(message);
                } catch { }
            }

            var tc = await user.Guild.GetTextChannelAsync(data.ChannelId);
            
            if (tc == null) return;
            else if (data.DeleteAfterMinutes < 1) {
                await tc.SendMessageAsync(message);
                return;
            }

            var msg = await tc.SendMessageAsync(message);

            await Task.Run(action: () => {
                Task.Delay((int)TimeSpan.FromMinutes(data.DeleteAfterMinutes).TotalMilliseconds);
                msg.DeleteAsync();
            });
        }

        private async Task JoinedGuildAsync(SocketGuild sGuild) 
            => await _log.LogToBotLogAsync($"<{sGuild.Name} ({sGuild.Id})> Joined", BotLogType.GuildManager);

        private async Task LeftGuildAsync(SocketGuild guild) {
            if (_playerManager.IsCreated(guild.Id)) _playerManager.GetPlayer(guild.Id).Player.CancelStream();

            using (var scope = _services.CreateScope()) {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var data = await db.ServerMusics.GetAsync(guild.Id);

                if (data != null && data.PlaylistId != ObjectId.Empty)
                    await _musicService.Playlist.DeleteAsync(data.PlaylistId);

                await db.DeleteGuildDataAsync(guild.Id);
            }

            await _log.LogToBotLogAsync($"<{guild.Name} ({guild.Id})> Left", BotLogType.GuildManager);
        }

        private async Task UserJoinedAsync(SocketGuildUser user) {
            ServerJoinLeave data;

            using (var scope = _services.CreateScope()) {
                data = await scope.ServiceProvider.GetService<TreeDiagramContext>().ServerJoinLeaves.GetAsync(user.Guild.Id);
            }

            if (data == null) return;

            var notification = data.GetMessage(MsgType.Join);

            if (string.IsNullOrEmpty(notification)) return;

            notification = notification.Replace("<server>", user.Guild.Name)
                .Replace("<user>", await _commandUtils.GetUsernameOrMentionAsync(user));
            
            await SendJoinLeaveMessageAsync(data, user, notification);
        }

        private async Task UserLeftAsync(SocketGuildUser user) {
            ServerJoinLeave data;

            using (var scope = _services.CreateScope()) {
                data = await scope.ServiceProvider.GetService<TreeDiagramContext>().ServerJoinLeaves.GetAsync(user.Guild.Id);
            }

            if (data == null) return;

            var notification = data.GetMessage(MsgType.Leave);

            if (!string.IsNullOrEmpty(notification)) notification = notification.Replace("<user>", user.Username);

            await SendJoinLeaveMessageAsync(data, user, notification);
        }
    
        private async Task UserVoiceStateUpdatedAsync(SocketUser sUser, SocketVoiceState a1, SocketVoiceState after) {
            if (sUser.IsBot || after.VoiceChannel == null) return;

            var guild = (IGuild)after.VoiceChannel.Guild;
            var user = await guild.GetUserAsync(sUser.Id);

            if (_playerManager.IsCreated(guild.Id) || user.VoiceChannel == null) return;

            ServerMusic data;

            using (var scope = _services.CreateScope()) {
                data = await scope.ServiceProvider.GetService<TreeDiagramContext>().ServerMusics.GetAsync(guild.Id);
            }

            if (data == null) return;

            var tc = data.AutoTextChannel != 0 ? await guild.GetTextChannelAsync(data.AutoTextChannel) : null;
            var vc = user.VoiceChannel;

            if (vc.Id == data.AutoVoiceChannel && tc != null) await _playerManager.CreatePlayerAsync(user, vc, tc, true);
        }

        private async Task ShardReadyAsync(DiscordSocketClient sClient) {
            if (_playerManager.PlayerContainers.Count > 0)
                foreach (var player in _playerManager.PlayerContainers) player.Player.CancelStream();

            if (!_shardsReady.ContainsKey(sClient.ShardId)) _shardsReady.Add(sClient.ShardId, false);
            else _shardsReady[sClient.ShardId] = true;

            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, $"SHARD {sClient.ShardId}", 
                $"Shard {(_shardsReady[sClient.ShardId] ? "Re-" : "")}Connected! ({sClient.Guilds.Count} Servers)"));
//             Await _timerManager.Initialize()

            if (_initialized) return;
            else if (_shardsReady.Count < _client.Shards.Count) return;

            _initialized = true;
            _serverCount.PreviousGuildCount = _client.Guilds.Count;

            await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help || {_client.Guilds.Count} Servers!", 
                type:ActivityType.Watching);
        }
    }
}