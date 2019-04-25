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
using Railgun.Utilities;
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
				var tc = (ITextChannel)Context.Channel;
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
			if (!await SystemUtilities.CheckIfSelfIsHigherRole(Context.Guild, user)) {
				await ReplyAsync($"Unable to kick {user.Username} as my role isn't high enough.");

				return;
			}

			await user.KickAsync(reason);
			await ReplyAsync($"{Format.Bold(user.Username)} has been kicked from the server. Reason: {Format.Bold(reason)}");

			var output = new StringBuilder()
				.AppendFormat("User Kicked {0} <{1} ({2})> {3}#{4}", SystemUtilities.GetSeparator, Context.Guild.Name, Context.Guild.Id, user.Username, user.DiscriminatorValue).AppendLine().AppendFormat("---- Reason : {0}", reason);

			await _botLog.SendBotLogAsync(BotLogType.Common, output.ToString());
		}

		[Command("ban"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)]
		public async Task BanAsync(IGuildUser user, [Remainder] string reason = "No Reason Specified")
		{
			var data = Context.Database.ServerWarnings.GetData(Context.Guild.Id);

			if (!await SystemUtilities.CheckIfSelfIsHigherRole(Context.Guild, user)) {
				await ReplyAsync($"Unable to ban {user.Username} as my role isn't high enough.");

				return;
			}

			await Context.Guild.AddBanAsync(user, 7, reason);

			if (data != null && data.Warnings.Any(find => find.UserId == user.Id))
				data.ResetWarnings(user.Id);

			await ReplyAsync($"{Format.Bold(user.Username)} has been banned from the server. Reason: {Format.Bold(reason)}");

			var output = new StringBuilder()
				.AppendFormat("User Banned {0} <{1} ({2})> {3}#{4}", SystemUtilities.GetSeparator, Context.Guild.Name, Context.Guild.Id, user.Username, user.DiscriminatorValue).AppendLine().AppendFormat("---- Reason : {0}", reason);

			await _botLog.SendBotLogAsync(BotLogType.Common, output.ToString());
		}

		[Command("unban"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)]
		public async Task UnbanAsync(ulong user)
		{
			await Context.Guild.RemoveBanAsync(user);
			await ReplyAsync($"User ID {Format.Bold(user.ToString())} is now unbanned from the server.");
			await _botLog.SendBotLogAsync(BotLogType.Common, $"User Unbanned {SystemUtilities.GetSeparator} <{Context.Guild.Name} ({Context.Guild.Id})> ID : {user}");
		}

		[Command("banlist"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers), BotPerms(ChannelPermission.AttachFiles)]
		public async Task BanlistAsync()
		{
			var bans = await Context.Guild.GetBansAsync();
			var output = new StringBuilder()
				.AppendLine("Guild Banned Users List:").AppendLine();

			if (bans.Count > 0) foreach (var ban in bans)
					output.AppendFormat("{0} ({1}) => [{2}]", ban.User.Username, ban.User.Id, ban.Reason).AppendLine();
			else output.AppendLine("Empty.");

			output.AppendLine().AppendLine("End Of Banned User List!");

			if (output.Length < 1950) {
				await ReplyAsync(output.ToString());

				return;
			}

			await (Context.Channel as ITextChannel).SendStringAsFileAsync("Banlist.txt", output.ToString(), "Ban List");
		}
	}
}