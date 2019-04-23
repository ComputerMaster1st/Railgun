using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Utilities;

namespace Railgun.Commands
{
    [Alias("zap")]
    public class Zap : SystemBase
    {
        private readonly IDiscordClient _client;
        private readonly CommandUtils _commandUtils;

        public Zap(IDiscordClient client, CommandUtils commandUtils) {
            _client = client;
            _commandUtils = commandUtils;
        }

        [Command]
        public Task ZapAsync() => ZapAsync((IGuildUser)Context.Author);
        
        [Command]
        public async Task ZapAsync(IGuildUser user) {
            if (user != null && user.Id == _client.CurrentUser.Id) {
                await ReplyAsync("I'm immune to electricity, BAKA!");
                return;
            }
            
            var name = _commandUtils.GetUsernameOrMention(user ?? (IGuildUser)Context.Author);

            await ReplyAsync($"{Format.Bold(name)} has been electrocuted! Something smells nice doesn't it?");
        }
    }
}