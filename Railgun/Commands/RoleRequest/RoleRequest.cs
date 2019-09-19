using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Commands.RoleRequest
{
    [Alias("role"), BotPerms(GuildPermission.ManageRoles)]
    public class RoleRequest : SystemBase
    {
        [Command]
        public async Task RoleAsync(string role)
        {
            
        }

        [Command]
        public async Task RoleAsync(IRole role)
        {

        }
    }
}