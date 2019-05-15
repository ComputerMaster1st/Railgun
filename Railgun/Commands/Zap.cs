using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands
{
    [Alias("zap")]
    public class Zap : SystemBase
    {
        private readonly IDiscordClient _client;

        public Zap(IDiscordClient client) => _client = client;
        
        [Command]
        public Task ZapAsync(IGuildUser user) 
        {
            if (user != null && user.Id == _client.CurrentUser.Id) return ReplyAsync("I'm immune to electricity, BAKA!");
            var name = SystemUtilities.GetUsernameOrMention(Context.Database, user ?? Context.Author as IGuildUser);
            return ReplyAsync($"{Format.Bold(name)} has been electrocuted! Something smells nice doesn't it?");
        }

        [Command]
        public Task ZapAsync() => ZapAsync(Context.Author as IGuildUser);
    }
}