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
        public Task ZapAsync() => ZapAsync((IGuildUser)Context.Author);
        
        [Command]
        public async Task ZapAsync(IGuildUser user) 
        {
            if (user != null && user.Id == _client.CurrentUser.Id) 
            {
                await ReplyAsync("I'm immune to electricity, BAKA!");
                return;
            }
            
            var name = SystemUtilities.GetUsernameOrMention(Context.Database, user ?? (IGuildUser)Context.Author);

            await ReplyAsync($"{Format.Bold(name)} has been electrocuted! Something smells nice doesn't it?");
        }
    }
}