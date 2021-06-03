using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands
{
    [Alias("zap")]
    public class Zap : SystemBase
    {
        [Command]
        public Task ExecuteAsync(IGuildUser user) 
        {
            var client = Context.Client as DiscordShardedClient;

            if (user != null && user.Id == client.CurrentUser.Id) 
                return ReplyAsync("I'm immune to electricity, BAKA!");

            var name = SystemUtilities.GetUsernameOrMention(Context.Database, user ?? Context.Author as IGuildUser);

            return ReplyAsync(string.Format("{0} has been electrocuted! Something smells nice doesn't it?", Format.Bold(name)));
        }

        [Command]
        public Task ExecuteAsync() => ExecuteAsync(Context.Author as IGuildUser);
    }
}