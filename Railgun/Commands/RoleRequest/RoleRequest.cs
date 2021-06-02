using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.RoleRequest
{
    [Alias("role"), BotPerms(GuildPermission.ManageRoles)]
    public partial class RoleRequest : SystemBase
    {
        [Command]
        public async Task RoleAsync(IRole role)
        {
            if (role is null)
            {
                await ReplyAsync("The role you requested does not exist. Please double-check in-case you mistyped. " +
                                 "If it is correct, please contact your mod/admin to resolve this.");
                return;
            }
            
            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.RoleRequest;

            if (data.RoleIds.Count == 0)
            {
                await ReplyAsync("Role-Request has either not been setup or no roles are available. " +
                                 "Please contact the server mod/admin to set it up.");
                return;
            }
            if (data.RoleIds.All(x => x != role.Id))
            {
                await ReplyAsync($"The role \"{Format.Bold(role.Name)}\" is not a public role. " +
                                 "Please check the list of publicly available roles.");
                return;
            }

            var user = (IGuildUser)Context.Author;

            if (user.RoleIds.Contains(role.Id))
            {
                await user.RemoveRoleAsync(role);
                await ReplyAsync($"Role \"{Format.Bold(role.Name)}\" removed.");
                return;
            }
            
            await user.AddRoleAsync(role);
            await ReplyAsync($"Role \"{Format.Bold(role.Name)}\" assigned.");
        }

        [Command]
        public Task RoleAsync([Remainder] string roleName)
            => RoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)));

        [Command("list")]
        public async Task ListAsync()
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
                .AppendFormat("{0} Publicly available roles on {1}", Format.Bold(data.RoleIds.Count.ToString()), 
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

            foreach (var id in badIds) data.RemoveRole(id);

            await ReplyAsync(output.ToString());
        }
    }
}