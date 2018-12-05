using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core.Commands;

namespace Railgun.Commands.Utilities
{
    [Alias("ping")]
    public class Ping : SystemBase
    {
        private readonly DiscordShardedClient _client;

        public Ping(DiscordShardedClient client) => _client = client;

        [Command]
        public Task PingAsync()
            => ReplyAsync($"Discord Client Latency : {Format.Bold(_client.Latency.ToString())}ms");

        [Alias("hello")]
        public class Hello : SystemBase {
            [Command]
            public Task HelloAsync()
            => ReplyAsync($"Hellew!");
        }
    }
}