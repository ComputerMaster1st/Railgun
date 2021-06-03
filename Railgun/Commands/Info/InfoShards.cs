using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Info
{
    public partial class Info
    {
        [Alias("shards")]
        public class InfoShards : SystemBase
        {
            private readonly DiscordShardedClient _client;

            public InfoShards(DiscordShardedClient client)
                => _client = client;

            [Command]
            public Task ExecuteAsync()
            {
                var output = new StringBuilder()
                    .AppendLine("Shard Status")
                    .AppendLine();

                foreach (var shard in _client.Shards)
                    output.AppendFormat("SHARD #{0} => Connection State: {1}, Latency: {2}, Servers: {3}", 
                        shard.ShardId, 
                        shard.ConnectionState, 
                        shard.Latency, 
                        shard.Guilds.Count).AppendLine();

                return ReplyAsync(Format.Code(output.ToString()));
            }
        }
    }
}
