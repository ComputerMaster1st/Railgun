using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands
{
    [Alias("hello")]
    public class Hello : SystemBase
    {        
        [Command]
        public async Task HelloAsync() {
            var name = SystemUtilities.GetUsernameOrMention(Context.Database, (IGuildUser)Context.Author);
            await ReplyAsync($"Hello {Format.Bold(name)}, I'm Railgun! Here to shock your world!");
        }
    }
}