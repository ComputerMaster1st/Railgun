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
        [Command]
        public async Task SayAsync([Remainder] string test) {
            var output = new StringBuilder().AppendFormat("{0}", test);
            
            // if (args != null) foreach (var other in args) output.AppendFormat(" {0}", other);

            await ReplyAsync(output.ToString());
        }
    }
}