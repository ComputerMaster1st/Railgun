using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Info
{
    public partial class Info
    {
        [Alias("dev")]
        public class InfoDev : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var output = new StringBuilder()
                    .AppendFormat("Railgun has been written by {0}.", Format.Bold("ComputerMaster1st")).AppendLine()
                    .AppendFormat("If you have any problems, issues, suggestions, etc, {0} can be found on this discord: {1}", Format.Bold("ComputerMaster1st"), Format.Bold("<https://discord.gg/Czw5ffx>")).AppendLine();

                return ReplyAsync(output.ToString());
            }
        }
    }
}
