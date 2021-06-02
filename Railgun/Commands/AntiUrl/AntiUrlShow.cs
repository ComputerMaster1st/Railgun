using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiUrl
{
    public partial class AntiUrl
    {
		[Alias("show"), BotPerms(ChannelPermission.AttachFiles)]
        public class AntiUrlShow : SystemBase
        {
			[Command]
			public async Task ExecuteAsync()
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
				else
				{
					var deletedChannels = new List<ulong>();

					foreach (var channelId in data.IgnoredChannels)
					{
						var tc = await Context.Guild.GetTextChannelAsync(channelId);

						if (tc == null) deletedChannels.Add(channelId);
						else if (initial)
						{
							output.AppendFormat("Ignored Channels : #{0}", tc.Name).AppendLine();
							initial = false;
						}
						else output.AppendFormat("                 : #{0}", tc.Name).AppendLine();
					}

					if (deletedChannels.Count > 0) deletedChannels.ForEach(channel => data.RemoveIgnoreChannel(channel));
				}

				if (data.BannedUrls.Count < 1)
					output.AppendFormat("    {0} Urls : None", data.DenyMode ? "Allowed" : "Blocked").AppendLine();
				else
				{
					initial = true;

					foreach (var url in data.BannedUrls)
					{
						if (initial)
						{
							output.AppendFormat("    {0} Urls : {1}", data.DenyMode ? "Allowed" : "Blocked", url).AppendLine();
							initial = false;
						}
						else output.AppendFormat("                 : {0}", url).AppendLine();
					}
				}

				if (output.Length > 1900)
					await (Context.Channel as ITextChannel).SendStringAsFileAsync("AntiUrl.txt", output.ToString());
				else await ReplyAsync(Format.Code(output.ToString()));
			}
		}
    }
}
