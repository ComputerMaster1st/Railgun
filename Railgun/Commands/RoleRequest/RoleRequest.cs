using System.Linq;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.RoleRequest
{
    [Alias("role"), BotPerms(GuildPermission.ManageRoles)]
    public class RoleRequest : SystemBase
    {
        [Command]
        public Task RoleAsync(string roleName) 
            => RoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName));

        [Command]
        public async Task RoleAsync(IRole role)
        {
            var data = Context.Database.ServerRoleRequests.GetData(Context.Guild.Id);
            if (data is null || data.RoleIds.Count == 0)
            {
                await ReplyAsync("Role-Request has either not been setup or no roles are available. " +
                                 "Please contact the server mod/admin to set it up.");
                return;
            }

            if (data.RoleIds.All(x => x.RoleId != role.Id))
            {
                await ReplyAsync($"The role \"{Format.Bold(role.Name)}\" is not a public role. " +
                                 $"Please check the list of publicly available roles.");
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
    }
}