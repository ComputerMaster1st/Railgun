using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Configuration;

namespace Railgun.Core.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class BotPerms : PreconditionAttribute
    {
        private readonly GuildPermission? _guildPermission = null;
        private readonly ChannelPermission? _channelPermission = null;

        public BotPerms(GuildPermission guildPermission) => _guildPermission = guildPermission;

        public BotPerms(ChannelPermission channelPermission) => _channelPermission = channelPermission;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            var config = services.GetService<MasterConfig>();
            var self = await context.Guild.GetCurrentUserAsync();

            if (_guildPermission.HasValue) {
                if (!(self.GuildPermissions.Has(_guildPermission.Value) || self.GuildPermissions.Administrator))
                    return await Task.FromResult(PreconditionResult.FromError($"I do not have permission to perform this command! {Format.Bold($"SERVER-PERM-MISSING : {_guildPermission.ToString()}")}"));
            } else if (_channelPermission.HasValue) {
                var channelPerms = self.GetPermissions((IGuildChannel)context.Channel);

                if (!(channelPerms.Has(_channelPermission.Value) || self.GuildPermissions.Administrator))
                    return await Task.FromResult(PreconditionResult.FromError($"I do not have permission to perform this command! {Format.Bold($"CHANNEL-PERM-MISSING : {_channelPermission.ToString()}")}"));
            }

            return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}