using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Server
{
	[Alias("warn")]
	public class Warning : SystemBase
	{
		private readonly Log _log;
		private readonly CommandUtils _commandUtils;

		public Warning(Log log, CommandUtils commandUtils)
		{
			_log = log;
			_commandUtils = commandUtils;
		}

		private Task WarnUserAsync(ServerWarning data, IUser user, string reason)
		{
			data.AddWarning(user.Id, reason);

			return ReplyAsync($"{user.Mention} has received a warning! Reason: {Format.Bold(reason)}");
		}

		[Command, UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)]
		public Task WarnAsync(IGuildUser user) => WarnAsync(user, "No Reason Specified");

		[Command, UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)]
		public async Task WarnAsync(IGuildUser user, [Remainder] string reason)
		{
			if (user.Id == Context.Client.CurrentUser.Id) {
				await ReplyAsync("You can not warn me. Just No. Baka.");

				return;
			}

			var data = Context.Database.ServerWarnings.GetOrCreateData(Context.Guild.Id);
			var userWarnings = data.GetWarnings(user.Id);

			if (data.WarnLimit < 1) {
				await ReplyAsync("User Warnings are currently disabled. You can enable it by changing the warning limit.");

				return;
			} else if (!await _commandUtils.CheckIfSelfIsHigherRole(Context.Guild, (IGuildUser)user)) {
				await ReplyAsync($"Unable to warn {user.Username} as my role isn't high enough to auto-ban the user.");

				return;
			} else if (userWarnings == null || userWarnings.Count < data.WarnLimit) {
				await WarnUserAsync(data, user, reason);

				return;
			}

			try {
				await Context.Guild.AddBanAsync(user, 7, reason);

				data.ResetWarnings(user.Id);

				await ReplyAsync($"{Format.Bold(user.Username)} has been Auto-Banned from the server. Reason: {Format.Bold($"{reason} & Too many warnings!")}");
				await _log.LogToBotLogAsync($"Auto Ban {Response.GetSeparator()} <{Context.Guild.Name} ({Context.Guild.Id})> Successful! {user.Username}#{user.DiscriminatorValue}", BotLogType.Common);
			} catch (Exception e) {
				await ReplyAsync("Unable to auto-ban user! Please be sure that I'm higher up on the role list.");

				var output = new StringBuilder()
					.AppendFormat("Auto Ban {0} <{1} ({2})> Failure!", Response.GetSeparator(), Context.Guild.Name, Context.Guild.Id).AppendLine().AppendFormat("---- Reason : {0}", e.Message);

				await _log.LogToBotLogAsync(output.ToString(), BotLogType.Common);
			}
		}

		[Command("list"), UserPerms(GuildPermission.BanMembers), BotPerms(ChannelPermission.AttachFiles)]
		public async Task ListAsync()
		{
			var data = Context.Database.ServerWarnings.GetData(Context.Guild.Id);

			if (data == null || data.Warnings.Count < 1) {
				await ReplyAsync("There are currently no users with warnings.");

				return;
			}

			var unknownUsers = new List<ulong>();
			var output = new StringBuilder()
				.AppendFormat("There are currently {0} user(s) with warnings for...", Format.Bold(data.Warnings.Count.ToString())).AppendLine().AppendLine();

			foreach (var warning in data.Warnings) {
				var user = await Context.Guild.GetUserAsync(warning.UserId);

				if (user == null) {
					unknownUsers.Add(warning.UserId);

					continue;
				}

				output.AppendFormat("---- {0} ({1})", Format.Bold($"{user.Username}#{user.DiscriminatorValue}"), warning.Reasons.Count).AppendLine();

				warning.Reasons.ForEach(reason => output.AppendFormat("    ---- {0}", Format.Bold(reason)).AppendLine());

				output.AppendLine();
			}

			if (unknownUsers.Count > 0) {
				unknownUsers.ForEach(data.ResetWarnings);

				output.AppendFormat("Detected {0} unknown user(s)! These user(s) have been automatically removed from the list.", unknownUsers.Count).AppendLine();
			}

			if (output.Length > 1950) {
				await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "Warnings.txt", output.ToString());

				return;
			}

			await ReplyAsync(output.ToString());
		}

		[Command("mylist")]
		public async Task MyListAsync()
		{
			var data = Context.Database.ServerWarnings.GetData(Context.Guild.Id);

			if (data == null || data.Warnings.Count < 1) {
				await ReplyAsync("There are currently no users with warnings.");

				return;
			}

			var warnings = data.GetWarnings(Context.Author.Id);

			if (warnings == null || warnings.Count < 1) {
				await ReplyAsync("You have no warnings to your name.");

				return;
			}

			var output = new StringBuilder()
				.AppendFormat("You have been warned {0} time(s) for...", Format.Bold(warnings.Count.ToString())).AppendLine().AppendLine();

			warnings.ForEach(reason => output.AppendFormat("---- {0}", Format.Bold(reason)).AppendLine());

			await ReplyAsync(output.ToString());
		}

		[Command("clear"), UserPerms(GuildPermission.BanMembers)]
		public async Task ClearAsync(IUser user)
		{
			var data = Context.Database.ServerWarnings.GetData(Context.Guild.Id);

			if (data == null || data.Warnings.Count < 1) {
				await ReplyAsync($"There are no warnings currently issued to {user.Mention}.");

				return;
			}

			var warnings = data.GetWarnings(user.Id);

			if (warnings == null || warnings.Count < 1) {
				await ReplyAsync($"There are no warnings currently issued to {user.Mention}.");

				return;
			}

			data.ResetWarnings(user.Id);

			await ReplyAsync($"{user.Mention} no longer has any warnings.");
		}

		[Command("empty"), UserPerms(GuildPermission.ManageGuild)]
		public async Task EmptyAsync()
		{
			var data = Context.Database.ServerWarnings.GetData(Context.Guild.Id);

			if (data == null || data.Warnings.Count < 1) {
				await ReplyAsync("Warnings list is already empty.");

				return;
			}

			data.Warnings.Clear();

			await ReplyAsync("Warnings list is now empty.");
		}

		[Command("limit"), UserPerms(GuildPermission.ManageGuild)]
		public async Task WarnLimitAsync(int limit = 5)
		{
			var data = Context.Database.ServerWarnings.GetOrCreateData(Context.Guild.Id);

			if (limit < 0) {
				await ReplyAsync("The limit entered is invalid. Must be 0 or higher.");

				return;
			}

			string message;

			if (limit > 0) {
				message = $"Auto-Ban{(data.WarnLimit == 0 ? " is now enabled and the" : "")} warning limit is now set to {Format.Bold(limit.ToString())}. You can disable warnings by changing the limit to 0.";
				data.WarnLimit = limit;
			} else {
				data.WarnLimit = limit;
				message = "Auto-Ban has been disabled.";
			}

			await ReplyAsync(message);
		}

		[Command("reset"), UserPerms(GuildPermission.ManageGuild)]
		public async Task ResetAsync()
		{
			var data = Context.Database.ServerWarnings.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync("Warnings has no data to reset.");

				return;
			}

			Context.Database.ServerWarnings.Remove(data);

			await ReplyAsync("Warnings has been reset & disabled.");
		}
	}
}