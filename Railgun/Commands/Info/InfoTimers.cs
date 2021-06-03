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
        [Alias("timers")]
        public class InfoTimers : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var output = new StringBuilder()
                    .AppendLine("TreeDiagram Timers Status")
                    .AppendLine()
                    .AppendFormat("Remind Me   : {0}", Context.Database.TimerRemindMes.Count()).AppendLine()
                    .AppendFormat("Assign Role : {0}", Context.Database.TimerAssignRoles.Count()).AppendLine()
                    .AppendFormat("Kick User   : {0}", Context.Database.TimerKickUsers.Count());

                return ReplyAsync(Format.Code(output.ToString()));
            }
        }
    }
}
