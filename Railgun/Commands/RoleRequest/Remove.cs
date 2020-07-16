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
        [Alias("remove"), UserPerms(GuildPermission.ManageRoles)]
        public class Remove : SystemBase
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
            public Task RemoveAsync(IRole role)
            {
                if (role is null)
                    return ReplyAsync("The role you tried to remove does not exist. " +
                                      "Please double-check in-case you mistyped.");

                var data = GetData(Context.Guild.Id);
                if (data is null || data.RoleIds.Count == 0)
                    return ReplyAsync("Role-Request has either not been setup or no roles were available.");

                return ReplyAsync(data.RemoveRole(role.Id) ?
                    $"Role {Format.Bold(role.Name)} is no longer available for role-request." :
                    $"Role {Format.Bold(role.Name)} isn't listed on role-request.");
            }

            [Command()]
            public Task RemoveAsync([Remainder] string role)
                => RemoveAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == role));
        }
    }
}