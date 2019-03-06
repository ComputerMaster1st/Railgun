using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TreeDiagram.Models.Server.Inactivity;

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
		private readonly TimerManager _timerManager;
		private readonly InactivityManager _inactivityManager;

		private bool _initialized;
		private readonly Dictionary<int, bool> _shardsReady = new Dictionary<int, bool>();

		public Events(IServiceProvider services)
		{
			_services = services;

			_config = _services.GetService<MasterConfig>();
			_log = _services.GetService<Log>();
			_commandUtils = _services.GetService<CommandUtils>();
			_serverCount = _services.GetService<ServerCount>();
			_client = _services.GetService<DiscordShardedClient>();
			_musicService = _services.GetService<MusicService>();
			_playerManager = _services.GetService<PlayerManager>();
			_timerManager = _services.GetService<TimerManager>();
			_inactivityManager = _services.GetService<InactivityManager>();

            _client.MessageReceived += MessageReceivedAsync;
            _client.MessageUpdated += (oldMsg, newMsg, channel) => MessageReceivedAsync(newMsg);
            _client.JoinedGuild += JoinedGuildAsync;
			_client.LeftGuild += LeftGuildAsync;
			_client.UserJoined += UserJoinedAsync;
			_client.UserLeft += UserLeftAsync;
			_client.UserVoiceStateUpdated += UserVoiceStateUpdatedAsync;
			_client.ShardReady += ShardReadyAsync;
		}

		private async Task SendJoinLeaveMessageAsync(ServerJoinLeave data, IGuildUser user, string message)
		{
			if (string.IsNullOrEmpty(message)) return;
			if (data.SendToDM) {
				try {
					await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(message);
				} catch { // Ignored
				}

				return;
			}
			if (data.ChannelId == 0 && !data.SendToDM) return;

			var tc = await user.Guild.GetTextChannelAsync(data.ChannelId);

			if (tc == null) return;

			IUserMessage msg;

			try {
				msg = await tc.SendMessageAsync(message);
			} catch {
				var output = new StringBuilder()
					.AppendFormat("<{0} <{1}>> Missing Send Message Permission!", tc.Guild.Name, tc.GuildId).AppendLine()
					.AppendFormat("---- Channel Name : {0}", tc.Name).AppendLine()
					.AppendFormat("---- Channel ID   : {0}", tc.Id).AppendLine();

				await _log.LogToBotLogAsync(output.ToString(), BotLogType.TaskScheduler);
				return;
			}


			if (data.DeleteAfterMinutes > 0)
				await Task.Run(async () => {
					await Task.Delay((int)TimeSpan.FromMinutes(data.DeleteAfterMinutes).TotalMilliseconds);

					try {
						await msg.DeleteAsync();
					} catch {
						var output2 = new StringBuilder()
							.AppendFormat("<{0} <{1}>> Missing Delete Message Permission!", tc.Guild.Name, tc.GuildId).AppendLine()
							.AppendFormat("---- Channel Name : {0}", tc.Name).AppendLine()
							.AppendFormat("---- Channel ID   : {0}", tc.Id).AppendLine();

						await _log.LogToBotLogAsync(output2.ToString(), BotLogType.TaskScheduler);
					}
				});
		}

        private Task MessageReceivedAsync(SocketMessage sMessage)
        {
            if (!(sMessage is SocketUserMessage) || !(sMessage.Channel is SocketGuildChannel) || string.IsNullOrEmpty(sMessage.Content))
                return Task.CompletedTask;

            return Task.Factory.StartNew(async() => {
                var tc = (ITextChannel)sMessage.Channel;

                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                    var data = db.ServerInactivities.GetData(tc.GuildId);
                    var guild = tc.Guild;
                    var user = await guild.GetUserAsync(sMessage.Author.Id);

                    if (data == null) return;
	                if (!data.IsEnabled || data.InactiveDaysThreshold == 0 || data.InactiveRoleId == 0) return;
                    if (data.UserWhitelist.Any((f) => f.UserId == user.Id)) return;
                    foreach (var roleId in data.RoleWhitelist) if (user.RoleIds.Contains(roleId.RoleId)) return;

                    if (data.Users.Any((f) => f.UserId == user.Id))
                    {
	                    if (user.RoleIds.Contains(data.InactiveRoleId))
		                    await user.RemoveRoleAsync(guild.GetRole(data.InactiveRoleId));
	                    
                        data.Users.First(f => f.UserId == user.Id).LastActive = DateTime.Now;
                        return;
                    }

                    data.Users.Add(new UserActivityContainer(user.Id) { LastActive = DateTime.Now });
                }
            });
        }

		private async Task JoinedGuildAsync(SocketGuild sGuild)
			=> await _log.LogToBotLogAsync($"<{sGuild.Name} ({sGuild.Id})> Joined", BotLogType.GuildManager);

		private async Task LeftGuildAsync(SocketGuild guild)
		{
			if (_playerManager.IsCreated(guild.Id)) _playerManager.GetPlayer(guild.Id).Player.CancelStream();

			using (var scope = _services.CreateScope()) {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				var data = db.ServerMusics.GetData(guild.Id);

				if (data != null && data.PlaylistId != ObjectId.Empty)
					await _musicService.Playlist.DeleteAsync(data.PlaylistId);

				db.DeleteGuildData(guild.Id);
			}

			await _log.LogToBotLogAsync($"<{guild.Name} ({guild.Id})> Left", BotLogType.GuildManager);
		}

		private async Task UserJoinedAsync(SocketGuildUser user)
		{
			ServerJoinLeave data;

			using (var scope = _services.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				var inactivityData = db.ServerInactivities.GetData(user.Guild.Id);

				if (inactivityData != null && inactivityData.IsEnabled && inactivityData.InactiveDaysThreshold != 0 && 
				    inactivityData.InactiveRoleId != 0)
				{
					if (inactivityData.Users.Any(u => u.UserId == user.Id))
						inactivityData.Users.First(u => u.UserId == user.Id).LastActive = DateTime.Now;
					else inactivityData.Users.Add(new UserActivityContainer(user.Id) { LastActive = DateTime.Now });
				}
				
				data = db.ServerJoinLeaves.GetData(user.Guild.Id);
			}

			if (data == null) return;

			var notification = data.GetMessage(MsgType.Join);

			if (string.IsNullOrEmpty(notification)) return;

			notification = notification.Replace("<server>", user.Guild.Name)
				.Replace("<user>", _commandUtils.GetUsernameOrMention(user));

			await SendJoinLeaveMessageAsync(data, user, notification);
		}

		private async Task UserLeftAsync(SocketGuildUser user)
		{
			ServerJoinLeave data;

			using (var scope = _services.CreateScope()) {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				var inactivityData = db.ServerInactivities.GetData(user.Guild.Id);

				inactivityData?.Users.RemoveAll(u => u.UserId == user.Id);

				data = db.ServerJoinLeaves.GetData(user.Guild.Id);
			}

			if (data == null) return;

			var notification = data.GetMessage(MsgType.Leave);

			if (!string.IsNullOrEmpty(notification)) notification = notification.Replace("<user>", user.Username);

			await SendJoinLeaveMessageAsync(data, user, notification);
		}

		private async Task UserVoiceStateUpdatedAsync(SocketUser sUser, SocketVoiceState a1, SocketVoiceState after)
		{
			if (sUser.IsBot || after.VoiceChannel == null) return;

			var guild = (IGuild)after.VoiceChannel.Guild;
			var user = await guild.GetUserAsync(sUser.Id);

			if (_playerManager.IsCreated(guild.Id) || user.VoiceChannel == null) return;

			ServerMusic data;

			using (var scope = _services.CreateScope()) {
				data = scope.ServiceProvider.GetService<TreeDiagramContext>().ServerMusics.GetData(guild.Id);
			}

			if (data == null) return;

			var tc = data.AutoTextChannel != 0 ? await guild.GetTextChannelAsync(data.AutoTextChannel) : null;
			var vc = user.VoiceChannel;

			if (vc.Id == data.AutoVoiceChannel && tc != null) await _playerManager.CreatePlayerAsync(user, vc, tc, true);
		}

		private Task ShardReadyAsync(DiscordSocketClient sClient)
			=> Task.Factory.StartNew(async () => await ShardReadyTaskAsync(sClient));

		private async Task ShardReadyTaskAsync(DiscordSocketClient sClient) {
			if (_playerManager.PlayerContainers.Count > 0)
				foreach (var player in _playerManager.PlayerContainers) player.Player.CancelStream();

			if (!_shardsReady.ContainsKey(sClient.ShardId)) _shardsReady.Add(sClient.ShardId, false);
			else _shardsReady[sClient.ShardId] = true;

			await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, $"SHARD {sClient.ShardId}",
				$"Shard {(_shardsReady[sClient.ShardId] ? "Re-" : "")}Connected! ({sClient.Guilds.Count} Servers)"));
			await _timerManager.InitializeAsync();
			_inactivityManager.Initialize();

			if (_initialized) return;
			if (_shardsReady.Count < _client.Shards.Count) return;

			_initialized = true;
			_serverCount.PreviousGuildCount = _client.Guilds.Count;

			await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help {Response.GetSeparator()} {_client.Guilds.Count} Servers!",
				type: ActivityType.Watching);
			await _client.SetStatusAsync(UserStatus.Online);
		}
	}
}