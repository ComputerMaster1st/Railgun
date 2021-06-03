using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
		[Alias("kick"), UserPerms(GuildPermission.KickMembers), BotPerms(GuildPermission.KickMembers)]
        public class ServerKick : SystemBase
        {
			private readonly BotLog _botLog;

			public ServerKick(BotLog botLog)
				=> _botLog = botLog;

			[Command]
			public async Task ExecuteAsync(IGuildUser user, [Remainder] string reason)
			{
				if (!await SystemUtilities.CheckIfSelfIsHigherRoleAsync(Context.Guild, user))
				{
					await ReplyAsync($"Unable to kick {user.Username} as my role isn't high enough.");
					return;
				}

				await user.KickAsync(reason);

				await ReplyAsync($"{Format.Bold(user.Username)} has been kicked from the server. Reason: {Format.Bold(reason)}");

				var output = new StringBuilder()
					.AppendFormat("User Kicked {0} <{1} ({2})> {3}#{4}", 
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
