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
    public class UserPerms : Attribute, IPreconditionAttribute
    {
        private readonly GuildPermission? _guildPermission = null;
        private readonly ChannelPermission? _channelPermission = null;

        public UserPerms(GuildPermission permission) => _guildPermission = permission;

        public UserPerms(ChannelPermission permission) => _channelPermission = permission;

        public Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services)
        {
            var config = services.GetService<MasterConfig>();
            var user = context.Guild.GetUserAsync(context.Author.Id).GetAwaiter().GetResult();

            if (_guildPermission.HasValue) 
            {
                if (!(user.GuildPermissions.Has(_guildPermission.Value) || user.GuildPermissions.Administrator || config.DiscordConfig.OtherAdmins.Contains(user.Id) || user.Id == config.DiscordConfig.MasterAdminId))
                    return Task.FromResult(PreconditionResult.FromError($"You do not have permission to use this command! {Format.Bold($"SERVER-PERM-MISSING : {_guildPermission.ToString()}")}"));
            }
            else if (_channelPermission.HasValue)
            {
                var channelPerms = user.GetPermissions((IGuildChannel)context.Channel);
                if (!(channelPerms.Has(_channelPermission.Value) || user.GuildPermissions.Administrator || config.DiscordConfig.OtherAdmins.Contains(user.Id) || user.Id == config.DiscordConfig.MasterAdminId))
                    return Task.FromResult(PreconditionResult.FromError($"You do not have permission to use this command! {Format.Bold($"CHANNEL-PERM-MISSING : {_channelPermission.ToString()}")}"));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}