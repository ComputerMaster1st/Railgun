using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Railgun.Core.Configuration;

namespace Railgun.Core
{
    public class Initializer
    {
        private readonly MasterConfig _masterConfig;
        private readonly DiscordShardedClient _client;

        public Initializer(MasterConfig config, DiscordShardedClient client) {
            _masterConfig = config;
            _client = client;
        }

        public async Task InitializeCommandsAsync() {
            throw new NotImplementedException();
        }
    }
}