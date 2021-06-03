using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("prefix")]
        public class RootPrefix : SystemBase
        {
            private readonly MasterConfig _config;

            public RootPrefix(MasterConfig config)
                => _config = config;

            [Command]
            public async Task ExecuteAsync([Remainder] string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    await ReplyAsync("Please specify a prefix.");
                    return;
                }

                _config.AssignPrefix(input);

                var client = Context.Client as DiscordShardedClient;

                await client.SetGameAsync($"{_config.DiscordConfig.Prefix}help {SystemUtilities.GetSeparator} On {client.Guilds.Count} Servers!", type: ActivityType.Watching);

                await ReplyAsync($"Prefix {Format.Code($"{_config.DiscordConfig.Prefix}<command>")} is now set.");
            }
        }
    }
}
