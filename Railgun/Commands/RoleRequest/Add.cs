using System.Linq;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.RoleRequest
{
    public partial class RoleRequest
    {
        [Alias("add"), UserPerms(GuildPermission.ManageRoles)]
        public class Add : SystemBase
        {
            private ServerRoleRequest GetData(ulong guildId, bool create = false)
            {
                ServerProfile data;

                if (create)
                    data = Context.Database.ServerProfiles.GetOrCreateData(guildId);
                else {
                    data = Context.Database.ServerProfiles.GetData(guildId);

                    if (data == null) 
                        return null;
                }

                if (data.RoleRequest == null)
                    if (create)
                        data.RoleRequest = new ServerRoleRequest();
                
                return data.RoleRequest;
            }

            [Command()]
            public Task AddAsync(IRole role)
            {
                if (role is null)
                    return ReplyAsync("The role you tried to add does not exist. " +
                                      "Please double-check in-case you mistyped.");

                var data = GetData(Context.Guild.Id, true);

                return ReplyAsync(data.AddRole(role.Id) ?
                    $"Role {Format.Bold(role.Name)} is now available for role-request." :
                    $"Role {Format.Bold(role.Name)} is already available for role-request.");
            }

            [Command()]
            public Task AddAsync([Remainder] string role)
                => AddAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == role));
        }
    }
}