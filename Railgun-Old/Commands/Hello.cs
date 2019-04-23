using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Utilities;

namespace Railgun.Commands
{
    [Alias("hello")]
    public class Hello : SystemBase
    {
        private readonly CommandUtils _commandUtils;

        public Hello(CommandUtils commandUtils) => _commandUtils = commandUtils;
        
        [Command]
        public async Task HelloAsync() {
            var name = _commandUtils.GetUsernameOrMention((IGuildUser)Context.Author);
            await ReplyAsync($"Hello {Format.Bold(name)}, I'm Railgun! Here to shock your world!");
        }
    }
}