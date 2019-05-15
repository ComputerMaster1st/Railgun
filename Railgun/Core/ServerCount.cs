using System;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Railgun.Core.Configuration;

namespace Railgun.Core
{
    public class ServerCount
    {
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly Timer _timer = new Timer(TimeSpan.FromMinutes(30).TotalMilliseconds);

        public int PreviousGuildCount { get; set; } = 0;

        public ServerCount(MasterConfig config, DiscordShardedClient client) {
            _config = config;
            _client = client;
        }

        public void Start()
        {
            _timer.AutoReset = true;
            _timer.Elapsed += async (sender, args) => await Task.Factory.StartNew(async () => await UpdateServerCountAsync());
            _timer.Start();
        }

        private async Task UpdateServerCountAsync() {
            if (PreviousGuildCount == _client.Guilds.Count) return;
            PreviousGuildCount = _client.Guilds.Count;
            await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help {SystemUtilities.GetSeparator} {PreviousGuildCount} Servers!", type:ActivityType.Watching);
        }
    }
}