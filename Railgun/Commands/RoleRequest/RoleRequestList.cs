using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.RoleRequest
{
    public partial class RoleRequest
    {
        [Alias("list")]
        public class RoleRequestList : SystemBase
        {
            [Command]
            public async Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.RoleRequest;

                if (data.RoleIds.Count == 0)
                {
                    await ReplyAsync("Role-Request has either not been setup or no roles are available. " +
                                     "Please contact the server mod/admin to set it up.");
                    return;
                }

                var badIds = new List<ulong>();
                var output = new StringBuilder()
                    .AppendFormat("{0} Publicly available roles on {1}", 
                        Format.Bold(data.RoleIds.Count.ToString()),
                        Format.Bold(Context.Guild.Name)).AppendLine()
                    .AppendLine();

                foreach (var id in data.RoleIds)
                {
                    var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == id);

                    if (role is null)
                    {
                        badIds.Add(id);
                        continue;
                    }

                    output.AppendFormat("{0}", role.Name).AppendLine();
                }

                foreach (var id in badIds)
                    data.RemoveRole(id);

                await ReplyAsync(output.ToString());
            }
        }
    }
}
