using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("silent"), UserPerms(GuildPermission.ManageGuild)]
		public partial class MusicSilent : SystemBase { }
	}
}