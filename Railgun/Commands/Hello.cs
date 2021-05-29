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
        public Task ExecuteAsync() {
            var name = SystemUtilities.GetUsernameOrMention(Context.Database, Context.Author as IGuildUser);
            return ReplyAsync($"Hello {Format.Bold(name)}, I'm Railgun! Here to shock your world!");
        }
    }
}