using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("auto"), UserPerms(GuildPermission.ManageGuild)]
		public partial class MusicAuto : SystemBase { }
	}
}