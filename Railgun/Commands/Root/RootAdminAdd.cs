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
            [Alias("add")]
            public class RootAdminAdd : SystemBase
            {
                private readonly MasterConfig _config;

                public RootAdminAdd(MasterConfig config)
                    => _config = config;

                [Command]
                public Task ExecuteAsync(IUser user)
                {
                    if (_config.DiscordConfig.OtherAdmins.Contains(user.Id))
                        return ReplyAsync($"{Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} is already a Railgun Admin.");
                    else
                    {
                        _config.AssignAdmin(user.Id);
                        return ReplyAsync($"Assigned {Format.Bold($"{user.Username}#{user.DiscriminatorValue}")} as a Railgun Admin.");
                    }
                }
            }
        }
    }
}
