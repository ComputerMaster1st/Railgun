using System;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Railgun.Core.Configuration;

namespace Railgun.Core.Utilities
{
    public class ServerCount
    {
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;
        private Timer _timer = new Timer(TimeSpan.FromMinutes(30).TotalMilliseconds);

        public int PreviousGuildCount { get; set; } = 0;

        public ServerCount(MasterConfig config, DiscordShardedClient client) {
            _config = config;
            _client = client;

            _timer.AutoReset = true;
            _timer.Elapsed += async (sender, args) => await UpdateServerCountAsync();
            _timer.Start();
        }

        private async Task UpdateServerCountAsync() {
            if (PreviousGuildCount == _client.Guilds.Count) return;

            PreviousGuildCount = _client.Guilds.Count;

            await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help || {PreviousGuildCount} Servers!", type:ActivityType.Watching);
        }
    }
}