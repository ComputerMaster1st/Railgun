using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Inactivity;
using Railgun.Music;
using Railgun.Timers;
using TreeDiagram;

namespace Railgun.Events
{
    public class OnReadyEvent : IEvent
    {
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly InactivityController _inactivityController;
        //private readonly PlayerController _playerController;
        private readonly TimerController _timerController;
        private readonly ServerCount _count;
        private readonly IServiceProvider _services;

        private bool _initialized;
		private readonly Dictionary<int, bool> _shardsReady = new Dictionary<int, bool>();

        public OnReadyEvent(MasterConfig config, DiscordShardedClient client, ServerCount serverCount, IServiceProvider services)
        {
            _config = config;
            _client = client;
            _count = serverCount;
            _services = services;

            _inactivityController = services.GetService<InactivityController>();
            //_playerController = services.GetService<PlayerController>();
            _timerController = services.GetService<TimerController>();
        }

        public void Load() => _client.ShardReady += (socketClient) => Task.Factory.StartNew(async () => await ExecuteAsync(socketClient));

        private async Task ExecuteAsync(DiscordSocketClient sClient)
        {
    //        if (_playerController.PlayerContainers.Count > 0)
				//foreach (var player in _playerController.PlayerContainers) player.Player.CancelStream();

			if (!_shardsReady.ContainsKey(sClient.ShardId)) _shardsReady.Add(sClient.ShardId, false);
			else _shardsReady[sClient.ShardId] = true;

			SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, $"SHARD {sClient.ShardId}",
				$"Shard {(_shardsReady[sClient.ShardId] ? "Re-" : "")}Connected! ({sClient.Guilds.Count} Servers)"));

			if (_initialized) return;
			if (_shardsReady.Count < _client.Shards.Count) return;

			_initialized = true;
			_count.PreviousGuildCount = _client.Guilds.Count;

#if RELEASE
            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                db.CheckForBadConfigs(_client.Guilds.ToList().ConvertAll(new Converter<SocketGuild, ulong>(guild => {
                    return guild.Id;
                })));
            }
#endif

            await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help {SystemUtilities.GetSeparator} {_client.Guilds.Count} Servers!",
				type: ActivityType.Watching);
			await _client.SetStatusAsync(UserStatus.Online);

			_timerController.Initialize();
			_inactivityController.Initialize();
        }
    }
}