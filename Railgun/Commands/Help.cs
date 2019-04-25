using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands
{
    [Alias("help")]
    public class Help : SystemBase
    {
        [Command]
        public Task HelpAsync() {
            var output = new StringBuilder()
                .AppendFormat("For the list of commands, please visit our wiki @ {0}", Format.Bold("<https://github.com/ComputerMaster1st/Railgun/wiki>")).AppendLine()
                .AppendFormat("If you have any questions or need help, contact the developer via the developer's Discord: {0}", Format.Bold("<https://discord.gg/Czw5ffx>")).AppendLine();
            
            return ReplyAsync(output.ToString());
        }
    }
}