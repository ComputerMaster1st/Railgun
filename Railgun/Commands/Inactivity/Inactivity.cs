using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Inactivity
{
    [Alias("inactive-monitor", "inactive", "imon"), UserPerms(GuildPermission.ManageGuild), BotPerms(GuildPermission.ManageRoles)]
    public partial class Inactivity : SystemBase 
    {
        [Command]
        public Task ExecuteAsync()
            => throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
    }
}
