using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Configuration;

namespace Railgun.Commands.Root
{
    public partial class Root 
    {
        [Alias("admin")]
        public class RootAdmin : SystemBase
        {
            private readonly MasterConfig _config;

            public RootAdmin(MasterConfig config) => _config = config;
            
            [Command("add")]
            public async Task AddAsync(IUser user) {
                if (_config.DiscordConfig.OtherAdmins.Contains(user.Id))
                    await ReplyAsync($"{Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} is already a Railgun Admin.");
                else { 
                    await _config.AssignAdminAsync(user.Id);
                    await ReplyAsync($"Assigned {Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} as a Railgun Admin.");
                }
            }
            
            [Command("remove")]
            public async Task RemoveAsync(IUser user) {
                if (_config.DiscordConfig.OtherAdmins.Contains(user.Id)) {
                    await _config.RemoveAdminAsync(user.Id);
                    await ReplyAsync($"{Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} is no longer a Railgun Admin.");
                } else
                    await ReplyAsync($"{Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} was never a Railgun Admin.");
            }
        }
    }
}