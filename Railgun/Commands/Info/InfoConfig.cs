using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Info
{
    public partial class Info
    {
        [Alias("config", "configs")]
        public class InfoConfig : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var output = new StringBuilder();

                output.AppendLine("TreeDiagram Configuration Report")
                    .AppendLine()
                    .AppendFormat("  Servers/Guilds : {0}", Context.Database.ServerProfiles.Count()).AppendLine()
                    .AppendFormat("           Users : {0}", Context.Database.UserProfiles.Count()).AppendLine()
                    .AppendLine()
                    .AppendFormat("Assign Role Timers : {0}", Context.Database.TimerAssignRoles.Count()).AppendLine()
                    .AppendFormat("  Kick User Timers : {0}", Context.Database.TimerKickUsers.Count()).AppendLine()
                    .AppendFormat("  Remind Me Timers : {0}", Context.Database.TimerRemindMes.Count()).AppendLine()
                    .AppendLine()
                    .AppendLine("End of Report!");

                return ReplyAsync(Format.Code(output.ToString()));
            }
        }
    }
}
