using System;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Commands.Results;
using Railgun.Core.Configuration;

namespace Railgun.Core.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UserPerms : Attribute, IPreconditionAttribute
    {
        private readonly GuildPermission? _guildPermission = null;
        private readonly ChannelPermission? _channelPermission = null;

        public UserPerms(GuildPermission permission) => _guildPermission = permission;

        public UserPerms(ChannelPermission permission) => _channelPermission = permission;

        public async Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services) {
            var config = services.GetService<MasterConfig>();
            var user = await context.Guild.GetUserAsync(context.Author.Id);

            if (_guildPermission.HasValue) {
                if (!(user.GuildPermissions.Has(_guildPermission.Value) || user.GuildPermissions.Administrator || config.DiscordConfig.OtherAdmins.Contains(user.Id) || user.Id == config.DiscordConfig.MasterAdminId))
                    return PreconditionResult.FromError($"You do not have permission to use this command! {Format.Bold($"SERVER-PERM-MISSING : {_guildPermission.ToString()}")}");
            } else if (_channelPermission.HasValue) {
                var channelPerms = user.GetPermissions((IGuildChannel)context.Channel);

                if (!(channelPerms.Has(_channelPermission.Value) || user.GuildPermissions.Administrator || config.DiscordConfig.OtherAdmins.Contains(user.Id) || user.Id == config.DiscordConfig.MasterAdminId))
                    return PreconditionResult.FromError($"You do not have permission to use this command! {Format.Bold($"CHANNEL-PERM-MISSING : {_channelPermission.ToString()}")}");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}