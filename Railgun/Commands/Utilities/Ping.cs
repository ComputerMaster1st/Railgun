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
            => Context.Channel.SendMessageAsync($"Discord Client Latency : {Format.Bold(_client.Latency.ToString())}ms")
    }
}