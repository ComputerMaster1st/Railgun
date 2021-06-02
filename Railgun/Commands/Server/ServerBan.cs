using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
		[Alias("ban"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)]
		public class ServerBan : SystemBase
		{
			private readonly BotLog _botLog;

			public ServerBan(BotLog botLog)
				=> _botLog = botLog;

			[Command]
			public async Task ExecuteAsync(IGuildUser user, [Remainder] string reason)
			{
				var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

				if (!await SystemUtilities.CheckIfSelfIsHigherRoleAsync(Context.Guild, user))
				{
					await ReplyAsync($"Unable to ban {user.Username} as my role isn't high enough.");
					return;
				}

				await Context.Guild.AddBanAsync(user, 7, reason);

				if (data != null && data.Warning != null && data.Warning.Warnings.Any(find => find.UserId == user.Id))
					data.Warning.ResetWarnings(user.Id);

				await ReplyAsync($"{Format.Bold(user.Username)} has been banned from the server. Reason: {Format.Bold(reason)}");

				var output = new StringBuilder()
					.AppendFormat("User Banned {0} <{1} ({2})> {3}#{4}", 
						SystemUtilities.GetSeparator, 
						Context.Guild.Name, 
						Context.Guild.Id, 
						user.Username, 
						user.DiscriminatorValue).AppendLine()
					.AppendFormat("---- Reason : {0}", reason);

				await _botLog.SendBotLogAsync(BotLogType.Common, output.ToString());
			}

			[Command]
			public Task ExecuteAsync(IGuildUser user)
				=> ExecuteAsync(user, "No Reason Specified");
		}
	}
}
