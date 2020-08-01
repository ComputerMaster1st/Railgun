using System.Linq;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.RoleRequest
{
    public partial class RoleRequest
    {
        [Alias("remove"), UserPerms(GuildPermission.ManageRoles)]
        public class Remove : SystemBase
        {      
            [Command()]
            public Task RemoveAsync(IRole role)
            {
                if (role is null)
                    return ReplyAsync("The role you tried to remove does not exist. " +
                                      "Please double-check in-case you mistyped.");

                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.RoleRequest;

                if (data.RoleIds.Count == 0)
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