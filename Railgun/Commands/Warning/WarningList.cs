using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Warning
{
    public partial class Warning
    {
		[Alias("list"), UserPerms(GuildPermission.BanMembers), BotPerms(ChannelPermission.AttachFiles)]
        public class WarningList : SystemBase
        {
			[Command]
			public async Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Warning;

				if (data.Warnings.Count < 1)
				{
					await ReplyAsync("There are currently no users with warnings.");
					return;
				}

				var unknownUsers = new List<ulong>();
				var output = new StringBuilder()
					.AppendFormat("There are currently {0} user(s) with warnings for...", 
						Format.Bold(data.Warnings.Count.ToString())).AppendLine()
					.AppendLine();

				foreach (var warning in data.Warnings)
				{
					var user = await Context.Guild.GetUserAsync(warning.UserId);

					if (user == null)
					{
						unknownUsers.Add(warning.UserId);
						continue;
					}

					output.AppendFormat("---- {0} ({1})", 
						Format.Bold(
							string.Format("{0}#{1}",
								user.Username,
								user.DiscriminatorValue)), 
							warning.Reasons.Count).AppendLine();

					warning.Reasons.ForEach(reason => output.AppendFormat("    ---- {0}", Format.Bold(reason)).AppendLine());

					output.AppendLine();
				}

				if (unknownUsers.Count > 0)
				{
					unknownUsers.ForEach(data.ResetWarnings);

					output.AppendFormat("Detected {0} unknown user(s)! These user(s) have been automatically removed from the list.", 
						unknownUsers.Count).AppendLine();
				}

				if (output.Length > 1950)
				{
					await (Context.Channel as ITextChannel).SendStringAsFileAsync("Warnings.txt", output.ToString());
					return;
				}

				await ReplyAsync(output.ToString());
			}
		}
    }
}
