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
        [Alias("add"), UserPerms(GuildPermission.ManageRoles)]
        public class Add : SystemBase
        {
            [Command()]
            public Task AddAsync(IRole role)
            {
                if (role is null)
                    return ReplyAsync("The role you tried to add does not exist. " +
                                      "Please double-check in-case you mistyped.");

                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.RoleRequest;

                return ReplyAsync(data.AddRole(role.Id) ?
                    $"Role {Format.Bold(role.Name)} is now available for role-request." :
                    $"Role {Format.Bold(role.Name)} is already available for role-request.");
            }

            [Command()]
            public Task AddAsync([Remainder] string role)
                => AddAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name.Equals(role, System.StringComparison.OrdinalIgnoreCase)));
        }
    }
}