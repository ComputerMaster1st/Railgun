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
            _timer.Elapsed += (sender, args) => Task.Factory.StartNew(() => UpdateServerCountAsync());
            _timer.Start();
        }

        private void UpdateServerCountAsync() {
            if (PreviousGuildCount == _client.Guilds.Count) return;
            PreviousGuildCount = _client.Guilds.Count;
            _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help {SystemUtilities.GetSeparator} {PreviousGuildCount} Servers!", type:ActivityType.Watching).GetAwaiter();
        }
    }
}