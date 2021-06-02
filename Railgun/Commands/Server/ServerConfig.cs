using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Commands.Server
{
    public partial class Server
	{
		[Alias("config"), UserPerms(GuildPermission.ManageGuild)]
		public partial class ServerConfig : SystemBase { }
	}
}