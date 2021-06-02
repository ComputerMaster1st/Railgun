using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("updatestatus")]
        public class RootUpdateStatus : SystemBase
        {
            private readonly MasterConfig _config;

            [Command]
            public async Task ExecuteAsync()
            {
                var client = Context.Client as DiscordShardedClient;

                await client.SetGameAsync($"{_config.DiscordConfig.Prefix}help {SystemUtilities.GetSeparator} On {client.Guilds.Count} Servers!", type: ActivityType.Watching);

                await ReplyAsync("Playing Status has been updated!");
            }
        }
    }
}
