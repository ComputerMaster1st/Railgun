using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;
using Railgun.Core.Extensions;
using TreeDiagram;

namespace Railgun.Commands.Server
{
    [Alias("server")]
	public partial class Server : SystemBase
	{
		private readonly BotLog _botLog;

		public Server(BotLog botLog) => _botLog = botLog;

		[Command("leave"), UserPerms(GuildPermission.ManageGuild)]
		public async Task LeaveAsync()
		{
			await ReplyAsync("My presence is no longer required. Goodbye everyone!");
			await Task.Delay(500);
			await Context.Guild.LeaveAsync();
		}
	}
}