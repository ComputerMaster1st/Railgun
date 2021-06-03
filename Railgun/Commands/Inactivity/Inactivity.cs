using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Commands.Inactivity
{
    [Alias("inactive-monitor", "inactive", "imon"), UserPerms(GuildPermission.ManageGuild), BotPerms(GuildPermission.ManageRoles)]
    public partial class Inactivity : SystemBase { }
}
