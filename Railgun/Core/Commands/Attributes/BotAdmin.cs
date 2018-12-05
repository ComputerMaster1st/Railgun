using System;
using System.Threading.Tasks;
using Finite.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Commands.Results;
using Railgun.Core.Configuration;

namespace Railgun.Core.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BotAdmin : Attribute, IPreconditionAttribute
    {
        public async Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services) {
            var config = services.GetService<MasterConfig>();
            var user = await context.Guild.GetUserAsync(context.Author.Id);

            if (user.Id == config.DiscordConfig.MasterAdminId || config.DiscordConfig.OtherAdmins.Contains(user.Id))
                return PreconditionResult.FromSuccess();
            
            return PreconditionResult.FromError("You must be a Railgun Administrator to use this command.");
        }
    }
}