using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        public partial class RootAdmin
        {
            [Alias("remove")]
            public class RootAdminRemove : SystemBase
            {
                private readonly MasterConfig _config;

                public RootAdminRemove(MasterConfig config)
                    => _config = config;

                [Command]
                public Task ExecuteAsync(IUser user)
                {
                    if (_config.DiscordConfig.OtherAdmins.Contains(user.Id))
                    {
                        _config.RemoveAdmin(user.Id);
                        return ReplyAsync($"{Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} is no longer a Railgun Admin.");
                    }
                    else
                        return ReplyAsync($"{Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} was never a Railgun Admin.");
                }
            }
        }
    }
}
