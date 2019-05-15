using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands
{
	[Alias("anticaps"), UserPerms(GuildPermission.ManageMessages), BotPerms(GuildPermission.ManageMessages)]
	public class AntiCaps : SystemBase
	{
		[Command]
		public Task EnableAsync()
		{
			var data = Context.Database.FilterCapses.GetOrCreateData(Context.Guild.Id);
			data.IsEnabled = !data.IsEnabled;
			return ReplyAsync($"Anti-Caps is now {Format.Bold(data.IsEnabled ? "Enabled" : "Disabled")}.");
		}

		[Command("includebots")]
		public Task IncludeBotsAsync()
		{
			var data = Context.Database.FilterCapses.GetOrCreateData(Context.Guild.Id);
			data.IncludeBots = !data.IncludeBots;
			return ReplyAsync($"Anti-Caps is now {Format.Bold(data.IsEnabled ? "Monitoring" : "Ignoring")} bots.");
		}

		[Command("percent")]
		public Task PercentAsync(int percent)
		{
			if (percent < 50 || percent > 100) 
				return ReplyAsync("Anti-Caps Percentage must be between 50-100.");

			var data = Context.Database.FilterCapses.GetOrCreateData(Context.Guild.Id);
			data.Percentage = percent;
			if (!data.IsEnabled) data.IsEnabled = true;

			return ReplyAsync($"Anti-Caps is now set to trigger at {Format.Bold($"{percent}%")} sensitivity.");
		}

		[Command("minlength")]
		public Task MinLengthAsync(int length)
		{
			if (length < 0)
				return ReplyAsync("Please specify a minimum message length of 0 or above.");

			var data = Context.Database.FilterCapses.GetOrCreateData(Context.Guild.Id);
			data.Length = length;
			if (!data.IsEnabled) data.IsEnabled = true;

			return ReplyAsync($"Anti-Caps is now set to scan messages longer than {Format.Bold(length.ToString())} characters.");
		}

		[Command("ignore")]
		public Task IgnoreAsync(ITextChannel pChannel = null)
		{
			var tc = pChannel ?? Context.Channel as ITextChannel;
			var data = Context.Database.FilterCapses.GetOrCreateData(Context.Guild.Id);

			if (data.IgnoredChannels.Any(f => f.ChannelId == tc.Id)) {
				data.IgnoredChannels.RemoveAll(f => f.ChannelId == tc.Id);
				return ReplyAsync("Anti-Caps is now monitoring this channel.");
			} else {
				data.IgnoredChannels.Add(new IgnoredChannels(tc.Id));
				return ReplyAsync("Anti-Caps is no longer monitoring this channel.");
			}
		}

		[Command("show")]
		public async Task ShowAsync()
		{
			var data = Context.Database.FilterCapses.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync("There are no settings available for Anti-Caps. Currently disabled.");
				return;
			}

			var output = new StringBuilder()
				.AppendLine("Anti-Caps Settings").AppendLine()
				.AppendFormat("          Status : {0}", data.IsEnabled ? "Enabled" : "Disabled").AppendLine()
				.AppendFormat("    Monitor Bots : {0}", data.IncludeBots ? "Yes" : "No").AppendLine()
				.AppendFormat("     Sensitivity : {0}", data.Percentage).AppendLine()
				.AppendFormat("Min. Msg. Length : {0}", data.Length).AppendLine();

			if (data.IgnoredChannels.Count > 0) {
				var initial = true;
				var deletedChannels = new List<IgnoredChannels>();

				foreach (var channel in data.IgnoredChannels) {
					var tc = await Context.Guild.GetTextChannelAsync(channel.ChannelId);

					if (tc == null) {
						deletedChannels.Add(channel);
						continue;
					} else if (initial) {
						output.AppendFormat("Ignored Channels : #{0}", tc.Name).AppendLine();
						initial = false;
					} else output.AppendFormat("                 : #{0}", tc.Name).AppendLine();
				}

				if (deletedChannels.Count > 0)
					deletedChannels.ForEach(channel => data.IgnoredChannels.Remove(channel));
			} else output.AppendLine("Ignored Channels : None");

			await ReplyAsync(Format.Code(output.ToString()));
		}

		[Command("reset")]
		public Task ResetAsync()
		{
			var data = Context.Database.FilterCapses.GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync("Anti-Caps has no data to reset.");

			Context.Database.FilterCapses.Remove(data);
			return ReplyAsync("Anti-Caps has been reset & disabled.");
		}
	}
}