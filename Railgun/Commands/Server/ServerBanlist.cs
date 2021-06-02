using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
		[Alias("banlist"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers), BotPerms(ChannelPermission.AttachFiles)]
        public class ServerBanlist : SystemBase
        {
			[Command]
			public async Task ExecuteAsync()
			{
				var bans = await Context.Guild.GetBansAsync();
				var output = new StringBuilder()
					.AppendLine("Guild Banned Users List:").AppendLine();

				if (bans.Count > 0)
					foreach (var ban in bans)
						output.AppendFormat("{0} ({1}) => [{2}]", ban.User.Username, ban.User.Id, ban.Reason).AppendLine();
				else 
					output.AppendLine("Empty.");

				output.AppendLine().AppendLine("End Of Banned User List!");

				if (output.Length < 1950)
				{
					await ReplyAsync(output.ToString());
					return;
				}

				await (Context.Channel as ITextChannel).SendStringAsFileAsync("Banlist.txt", output.ToString(), "Ban List");
			}
		}
    }
}
