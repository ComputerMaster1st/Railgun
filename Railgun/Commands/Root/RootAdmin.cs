using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;

namespace Railgun.Commands.Root
{
    public partial class Root 
    {
        [Alias("admin")]
        public partial class RootAdmin : SystemBase
        {
            private readonly MasterConfig _config;

            public RootAdmin(MasterConfig config) => _config = config;



            [Command("remove")]
            public Task RemoveAsync(IUser user) {
                if (_config.DiscordConfig.OtherAdmins.Contains(user.Id)) {
                    _config.RemoveAdmin(user.Id);
                    return ReplyAsync($"{Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} is no longer a Railgun Admin.");
                } else
                    return ReplyAsync($"{Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} was never a Railgun Admin.");
            }
        }
    }
}