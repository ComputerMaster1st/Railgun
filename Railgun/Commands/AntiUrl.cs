using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.Filter;

namespace Railgun.Commands
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
		public Task EnableAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			data.IsEnabled = !data.IsEnabled;
			return ReplyAsync($"Anti-Url is now {Format.Bold(data.IsEnabled ? "Enabled" : "Disabled")}.");
		}

		[Command("includebots")]
		public Task IncludeBotsAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			data.IncludeBots = !data.IncludeBots;
			return ReplyAsync($"Anti-Url is now {Format.Bold(data.IncludeBots ? "Monitoring" : "Ignoring")} bots.");
		}

		[Command("invites")]
		public Task InvitesAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			data.BlockServerInvites = !data.BlockServerInvites;
			return ReplyAsync($"Anti-Url is now {Format.Bold(data.BlockServerInvites ? "Blocking" : "Allowing")} server invites.");
		}

		[Command("add")]
		public Task AddAsync(string url)
		{
			var newUrl = ProcessUrl(url);
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			if (data.BannedUrls.Contains(newUrl))
				return ReplyAsync("The Url specified is already listed.");

			data.BannedUrls.Add(newUrl);
			if (!data.IsEnabled) data.IsEnabled = true;

			return ReplyAsync($"The Url {Format.Bold(newUrl)} is now added to list.");
		}

		[Command("remove")]
		public Task RemoveAsync(string url)
		{
			var newUrl = ProcessUrl(url);
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			if (!data.BannedUrls.Contains(newUrl))
				return ReplyAsync("The Url specified is not listed.");

			data.BannedUrls.Remove(newUrl);
			return ReplyAsync($"The Url {Format.Bold(newUrl)} is now removed from list.");
		}

		[Command("ignore")]
		public Task IgnoreAsync(ITextChannel pChannel = null)
		{
			var tc = pChannel ?? (ITextChannel)Context.Channel;
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			if (data.IgnoredChannels.Any(f => f == tc.Id)) {
				data.IgnoredChannels.RemoveAll(f => f == tc.Id);
				return ReplyAsync("Anti-Url is now monitoring this channel.");
			} else {
				data.IgnoredChannels.Add(tc.Id);
				return ReplyAsync("Anti-Url is no longer monitoring this channel.");
			}
		}

		[Command("mode")]
		public Task ModeAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			data.DenyMode = !data.DenyMode;
			if (!data.IsEnabled) data.IsEnabled = true;
			return ReplyAsync($"Switched Anti-Url Mode to {(data.DenyMode ? Format.Bold("Deny") : Format.Bold("Allow"))}. {(data.DenyMode ? "Deny" : "Allow")} all urls except listed.");
		}

		[Command("show"), BotPerms(ChannelPermission.AttachFiles)]
		public async Task ShowAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			var initial = true;

			var output = new StringBuilder()
				.AppendLine("Anti-Url Settings").AppendLine()
				.AppendFormat("          Status : {0}", data.IsEnabled ? "Enabled" : "Disabled").AppendLine()
				.AppendFormat("            Mode : {0} All", data.DenyMode ? "Deny" : "Allow").AppendLine()
				.AppendFormat("   Block Invites : {0}", data.BlockServerInvites ? "Yes" : "No").AppendLine()
				.AppendFormat("    Monitor Bots : {0}", data.IncludeBots ? "Yes" : "No").AppendLine();

			if (data.IgnoredChannels.Count < 1) output.AppendLine("Ignored Channels : None");
			else {
				var deletedChannels = new List<ulong>();

				foreach (var channelId in data.IgnoredChannels) {
					var tc = await Context.Guild.GetTextChannelAsync(channelId);

					if (tc == null) deletedChannels.Add(channelId);
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
				await (Context.Channel as ITextChannel).SendStringAsFileAsync("AntiUrl.txt", output.ToString());
			else await ReplyAsync(Format.Code(output.ToString()));
		}

		[Command("reset")]
		public Task ResetAsync()
		{
			var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync("Anti-Url has no data to reset.");

			data.Filters.ResetUrls();
			return ReplyAsync("Anti-Url has been reset & disabled.");
		}
	}
}