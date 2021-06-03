using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Server
{
    public partial class Server
	{
		[Alias("config"), UserPerms(GuildPermission.ManageGuild)]
		public partial class ServerConfig : SystemBase
		{
			[Command]
			public Task ExecuteAsync()
				=> throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
		}
	}
}