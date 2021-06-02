using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiCaps
{
    public partial class AntiCaps
    {
		[Alias("show")]
        public class AntiCapsShow : SystemBase
        {
			[Command]
			public async Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Filters.Caps;

				var output = new StringBuilder()
					.AppendLine("Anti-Caps Settings").AppendLine()
					.AppendFormat("          Status : {0}", data.IsEnabled ? "Enabled" : "Disabled").AppendLine()
					.AppendFormat("    Monitor Bots : {0}", data.IncludeBots ? "Yes" : "No").AppendLine()
					.AppendFormat("     Sensitivity : {0}", data.Percentage).AppendLine()
					.AppendFormat("Min. Msg. Length : {0}", data.Length).AppendLine();

				if (data.IgnoredChannels.Count > 0)
				{
					var initial = true;
					var deletedChannels = new List<ulong>();

					foreach (var channelId in data.IgnoredChannels)
					{
						var tc = await Context.Guild.GetTextChannelAsync(channelId);

						if (tc == null)
						{
							deletedChannels.Add(channelId);
							continue;
						}
						else if (initial)
						{
							output.AppendFormat("Ignored Channels : #{0}", tc.Name).AppendLine();
							initial = false;
						}
						else output.AppendFormat("                 : #{0}", tc.Name).AppendLine();
					}

					if (deletedChannels.Count > 0)
						deletedChannels.ForEach(channel => data.RemoveIgnoreChannel(channel));
				}
				else output.AppendLine("Ignored Channels : None");

				await ReplyAsync(Format.Code(output.ToString()));
			}
		}
    }
}
