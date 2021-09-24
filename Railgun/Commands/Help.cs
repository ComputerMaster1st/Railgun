using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands
{
    [Alias("help")]
    public class Help : SystemBase
    {
        [Command]
        public Task ExecuteAsync()
        {
            var output = new StringBuilder()
                .AppendFormat("For the list of commands, please visit our wiki @ {0}", Format.Bold("<https://github.com/ComputerMaster1st/Railgun/wiki>")).AppendLine()
                .AppendFormat("If you have any questions or need help, contact the developer via the developer's Discord: {0}", Format.Bold("<https://discord.gg/fFbMNGbXVv>")).AppendLine()
                .AppendLine(Format.Bold("PLEASE NOTE: Railgun#2440 (261878358625746964) has been hijacked along with Nekoputer#0001 (166264102526779392). Please visit our new Discord server for any new updates!"));

            return ReplyAsync(output.ToString());
        }
    }
}