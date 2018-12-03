using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Railgun.Core.Configuration;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;

namespace Railgun.Core
{
    public class Initializer
    {
        private readonly MasterConfig _masterConfig;
        private readonly DiscordShardedClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private Log _log;
        private ServerCount _serverCount;

        public Initializer(MasterConfig config, DiscordShardedClient client) {
            _masterConfig = config;
            _client = client;
        }

        public async Task InitializeCommandsAsync() {
            throw new NotImplementedException();
        }
    }
}