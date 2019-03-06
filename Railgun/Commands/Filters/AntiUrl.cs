using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Filters
{
	[Alias("antiurl"), UserPerms(GuildPermission.ManageMessages), BotPerms(GuildPermission.ManageMessages)]
	public class AntiUrl : SystemBase
	{
		private string ProcessUrl(string url)
		{
			var cleanUrl = url;
			var parts = new string[] { "http://", "https://", "www." };

			foreach (var part in parts)
				if (cleanUrl.Contains(part)) cleanUrl = cleanUrl.Replace(part, "");

			return cleanUrl.Split('/', 2)[0];
		}

		[Command]
		public async Task EnableAsync()
		{
			var data = Context.Database.FilterUrls.GetOrCreateData(Context.Guild.Id);

			data.IsEnabled = !data.IsEnabled;

			await ReplyAsync($"Anti-Url is now {Format.Bold(data.IsEnabled ? "Enabled" : "Disabled")}.");
		}

		[Command("includebots")]
		public async Task IncludeBotsAsync()
		{
			var data = Context.Database.FilterUrls.GetOrCreateData(Context.Guild.Id);

			data.IncludeBots = !data.IncludeBots;

			await ReplyAsync($"Anti-Url is now {Format.Bold(data.IncludeBots ? "Monitoring" : "Ignoring")} bots.");
		}

		[Command("invites")]
		public async Task InvitesAsync()
		{
			var data = Context.Database.FilterUrls.GetOrCreateData(Context.Guild.Id);

			data.BlockServerInvites = !data.BlockServerInvites;

			await ReplyAsync($"Anti-Url is now {Format.Bold(data.BlockServerInvites ? "Blocking" : "Allowing")} server invites.");
		}

		[Command("add")]
		public async Task AddAsync(string url)
		{
			var newUrl = ProcessUrl(url);
			var data = Context.Database.FilterUrls.GetOrCreateData(Context.Guild.Id);

			if (data.BannedUrls.Contains(newUrl)) {
				await ReplyAsync("The Url specified is already listed.");
				return;
			}

			data.BannedUrls.Add(newUrl);

			if (!data.IsEnabled) data.IsEnabled = true;

			await ReplyAsync($"The Url {Format.Bold(newUrl)} is now added to list.");
		}

		[Command("remove")]
		public async Task RemoveAsync(string url)
		{
			var newUrl = ProcessUrl(url);
			var data = Context.Database.FilterUrls.GetData(Context.Guild.Id);

			if (data == null || !data.BannedUrls.Contains(newUrl)) {
				await ReplyAsync("The Url specified is not listed.");
				return;
			}

			data.BannedUrls.Remove(newUrl);

			await ReplyAsync($"The Url {Format.Bold(newUrl)} is now removed from list.");
		}

		[Command("ignore")]
		public async Task IgnoreAsync(ITextChannel pChannel = null)
		{
			var tc = pChannel ?? (ITextChannel)Context.Channel;
			var data = Context.Database.FilterUrls.GetOrCreateData(Context.Guild.Id);

			if (data.IgnoredChannels.Any(f => f.ChannelId == tc.Id)) {
				data.IgnoredChannels.RemoveAll(f => f.ChannelId == tc.Id);

				await ReplyAsync("Anti-Url is now monitoring this channel.");
			} else {
				data.IgnoredChannels.Add(new IgnoredChannels(tc.Id));

				await ReplyAsync("Anti-Url is no longer monitoring this channel.");
			}
		}

		[Command("mode")]
		public async Task ModeAsync()
		{
			var data = Context.Database.FilterUrls.GetOrCreateData(Context.Guild.Id);

			data.DenyMode = !data.DenyMode;

			if (!data.IsEnabled) data.IsEnabled = true;

			await ReplyAsync($"Switched Anti-Url Mode to {(data.DenyMode ? Format.Bold("Deny") : Format.Bold("Allow"))}. {(data.DenyMode ? "Deny" : "Allow")} all urls except listed.");
		}

		[Command("show"), BotPerms(ChannelPermission.AttachFiles)]
		public async Task ShowAsync()
		{
			var data = Context.Database.FilterUrls.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync("There are no settings available for Anti-Url. Currently disabled.");
				return;
			}

			var initial = true;

			var output = new StringBuilder()
				.AppendLine("Anti-Url Settings").AppendLine()
				.AppendFormat("          Status : {0}", data.IsEnabled ? "Enabled" : "Disabled").AppendLine()
				.AppendFormat("            Mode : {0} All", data.DenyMode ? "Deny" : "Allow").AppendLine()
				.AppendFormat("   Block Invites : {0}", data.BlockServerInvites ? "Yes" : "No").AppendLine()
				.AppendFormat("    Monitor Bots : {0}", data.IncludeBots ? "Yes" : "No").AppendLine();

			if (data.IgnoredChannels.Count < 1) output.AppendLine("Ignored Channels : None");
			else {
				var deletedChannels = new List<IgnoredChannels>();

				foreach (var channel in data.IgnoredChannels) {
					var tc = await Context.Guild.GetTextChannelAsync(channel.ChannelId);

					if (tc == null) deletedChannels.Add(channel);
					else if (initial) {
						output.AppendFormat("Ignored Channels : #{0}", tc.Name).AppendLine();
						initial = false;
					} else output.AppendFormat("                 : #{0}", tc.Name).AppendLine();
				}

				if (deletedChannels.Count > 0) deletedChannels.ForEach(channel => data.IgnoredChannels.Remove(channel));
			}

			if (data.BannedUrls.Count < 1)
				output.AppendFormat("    {0} Urls : None", data.DenyMode ? "Allowed" : "Blocked").AppendLine();
			else {
				initial = true;

				foreach (var url in data.BannedUrls) {
					if (initial) {
						output.AppendFormat("    {0} Urls : {1}", data.DenyMode ? "Allowed" : "Blocked", url).AppendLine();
						initial = false;
					} else output.AppendFormat("                 : {0}", url).AppendLine();
				}
			}

			if (output.Length > 1900)
				await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "AntiUrl.txt", output.ToString());
			else await ReplyAsync(Format.Code(output.ToString()));
		}

		[Command("reset")]
		public async Task ResetAsync()
		{
			var data = Context.Database.FilterUrls.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync("Anti-Url has no data to reset.");
				return;
			}

			Context.Database.FilterUrls.Remove(data);

			await ReplyAsync("Anti-Url has been reset & disabled.");
		}
	}
}