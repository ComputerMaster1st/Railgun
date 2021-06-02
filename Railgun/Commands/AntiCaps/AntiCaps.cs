using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.AntiCaps
{
    [Alias("anticaps"), UserPerms(GuildPermission.ManageMessages), BotPerms(GuildPermission.ManageMessages)]
	public partial class AntiCaps : SystemBase
	{
		[Command]
		public Task EnableAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Caps;

			data.IsEnabled = !data.IsEnabled;
			return ReplyAsync($"Anti-Caps is now {Format.Bold(data.IsEnabled ? "Enabled" : "Disabled")}.");
		}

		[Command("includebots")]
		public Task IncludeBotsAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Caps;

			data.IncludeBots = !data.IncludeBots;
			return ReplyAsync($"Anti-Caps is now {Format.Bold(data.IsEnabled ? "Monitoring" : "Ignoring")} bots.");
		}

		[Command("percent")]
		public Task PercentAsync(int percent)
		{
			if (percent < 50 || percent > 100) 
				return ReplyAsync("Anti-Caps Percentage must be between 50-100.");

			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Caps;

			data.Percentage = percent;
			if (!data.IsEnabled) data.IsEnabled = true;

			return ReplyAsync($"Anti-Caps is now set to trigger at {Format.Bold($"{percent}%")} sensitivity.");
		}
	}
}