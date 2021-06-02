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

		[Command("clear"), UserPerms(GuildPermission.ManageMessages), BotPerms(ChannelPermission.ManageMessages), BotPerms(GuildPermission.ReadMessageHistory)]
		public async Task ClearAsync(int count = 100)
		{
			var deleted = 0;

			while (count > 0) {
				var subCount = count >= 100 ? 100 : count;
				var tc = Context.Channel as ITextChannel;
				var msgs = await tc.GetMessagesAsync(subCount).FlattenAsync();
				var msgsToDelete = new List<IMessage>();

				foreach (var msg in msgs)
					if (msg.CreatedAt > DateTime.Now.AddDays(-13).AddHours(-23).AddMinutes(-50))
						msgsToDelete.Add(msg);

				await tc.DeleteMessagesAsync(msgsToDelete);

				deleted += msgsToDelete.Count;

				if (msgsToDelete.Count() == subCount) count -= subCount;
				else break;
			}

			await ReplyAsync($"Up to {Format.Bold(deleted.ToString())} messages have been deleted from the channel.");
		}

		[Command("id"), UserPerms(GuildPermission.ManageGuild)]
		public Task IdAsync() => ReplyAsync($"This server's ID is {Format.Bold(Context.Guild.Id.ToString())}");

		[Command("kick"), UserPerms(GuildPermission.KickMembers), BotPerms(GuildPermission.KickMembers)]
		public async Task KickAsync(IGuildUser user, [Remainder] string reason = "No Reason Specified")
		{
			if (!await SystemUtilities.CheckIfSelfIsHigherRoleAsync(Context.Guild, user)) {
				await ReplyAsync($"Unable to kick {user.Username} as my role isn't high enough.");
				return;
			}

			await user.KickAsync(reason);
			await ReplyAsync($"{Format.Bold(user.Username)} has been kicked from the server. Reason: {Format.Bold(reason)}");

			var output = new StringBuilder()
				.AppendFormat("User Kicked {0} <{1} ({2})> {3}#{4}", SystemUtilities.GetSeparator, Context.Guild.Name, Context.Guild.Id, user.Username, user.DiscriminatorValue).AppendLine().AppendFormat("---- Reason : {0}", reason);
			await _botLog.SendBotLogAsync(BotLogType.Common, output.ToString());
		}
	}
}