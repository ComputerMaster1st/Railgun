using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Configuration;

namespace Railgun.Core.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BotAdmin : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            var config = services.GetService<MasterConfig>();
            var user = await context.Guild.GetUserAsync(context.User.Id);

            if (user.Id == config.DiscordConfig.MasterAdminId || config.DiscordConfig.OtherAdmins.Contains(user.Id))
                return await Task.FromResult(PreconditionResult.FromSuccess());
            
            return await Task.FromResult(PreconditionResult.FromError("You must be a Railgun Administrator to use this command."));
        }
    }
}