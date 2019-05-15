using System;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Results;
using Railgun.Core.Configuration;

namespace Railgun.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class BotPerms : Attribute, IPreconditionAttribute
    {
        private readonly GuildPermission? _guildPermission = null;
        private readonly ChannelPermission? _channelPermission = null;

        public BotPerms(GuildPermission guildPermission) => _guildPermission = guildPermission;

        public BotPerms(ChannelPermission channelPermission) => _channelPermission = channelPermission;

        public async Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services)
        {
            var config = services.GetService<MasterConfig>();
            var self = await context.Guild.GetCurrentUserAsync();

            if (_guildPermission.HasValue)
                if (!(self.GuildPermissions.Has(_guildPermission.Value) || self.GuildPermissions.Administrator))
                    return PreconditionResult.FromError($"I do not have permission to perform this command! {Format.Bold($"SERVER-PERM-MISSING : {_guildPermission.ToString()}")}");
            if (_channelPermission.HasValue) {
                var channelPerms = self.GetPermissions((IGuildChannel)context.Channel);
                if (!(channelPerms.Has(_channelPermission.Value) || self.GuildPermissions.Administrator))
                    return PreconditionResult.FromError($"I do not have permission to perform this command! {Format.Bold($"CHANNEL-PERM-MISSING : {_channelPermission.ToString()}")}");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}