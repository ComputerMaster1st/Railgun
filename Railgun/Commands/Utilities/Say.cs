using System.Text;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;

namespace Railgun.Commands.Utilities
{
    [Alias("say"), BotAdmin]
    public class Say : SystemBase
    {
        [Command("remain")]
        public Task SayAsync([Remainder] string test) => ReplyAsync(test);

        [Command("non-remain")]
        public Task Say2Async(string test) => ReplyAsync(test);
    }
}