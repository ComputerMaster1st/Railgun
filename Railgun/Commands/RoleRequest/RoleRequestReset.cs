using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.RoleRequest
{
    public partial class RoleRequest
    {
        [Alias("reset")]
        public class RoleRequestReset : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

                if (data == null)
                    return ReplyAsync("Role-Request has no data to reset.");

                data.ResetRoleRequest();

                return ReplyAsync("Role-Request has been reset.");
            }
        }
    }
}
