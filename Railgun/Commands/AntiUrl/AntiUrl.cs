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

namespace Railgun.Commands.AntiUrl
{
    [Alias("antiurl"), UserPerms(GuildPermission.ManageMessages), BotPerms(GuildPermission.ManageMessages)]
	public partial class AntiUrl : SystemBase
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
				data.RemoveIgnoreChannel(tc.Id);
				return ReplyAsync("Anti-Url is now monitoring this channel.");
			} else {
				data.AddIgnoreChannel(tc.Id);
				return ReplyAsync("Anti-Url is no longer monitoring this channel.");
			}
		}
	}
}