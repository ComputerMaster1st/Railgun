using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Warning
{
    [Alias("warn")]
	public partial class Warning : SystemBase
	{
		private readonly BotLog _botLog;

		public Warning(BotLog botLog)
			=>_botLog = botLog;
		
		private Task WarnUserAsync(ServerWarning data, IUser user, string reason)
		{
			data.AddWarning(user.Id, reason);

			return ReplyAsync(string.Format("{0} has received a warning! Reason: {1}", user.Mention, Format.Bold(reason)));
		}

		[Command, UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)]
		public async Task ExecuteAsync(IGuildUser user, [Remainder] string reason)
		{
			if (user.Id == Context.Client.CurrentUser.Id) 
			{
				await ReplyAsync("You can not warn me. Just No. Baka.");
				return;
			}

			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Warning;
			var userWarnings = data.GetWarnings(user.Id);

			if (data.WarnLimit < 1) 
			{
				await ReplyAsync("User Warnings are currently disabled. You can enable it by changing the warning limit.");
				return;
			} 

			if (!await SystemUtilities.CheckIfSelfIsHigherRoleAsync(Context.Guild, user)) 
			{
				await ReplyAsync(string.Format("Unable to warn {0} as my role isn't high enough to auto-ban the user.", user.Username));
				return;
			} 

			if (userWarnings == null || userWarnings.Count < data.WarnLimit) 
			{
				await WarnUserAsync(data, user, reason);
				return;
			}

			try 
			{
				await Context.Guild.AddBanAsync(user, 7, reason);

				data.ResetWarnings(user.Id);

				await ReplyAsync(string.Format("{0} has been Auto-Banned from the server. Reason: {1}", 
					Format.Bold(user.Username), 
					Format.Bold(string.Format("{0} & Too many warnings!", reason))));

				await _botLog.SendBotLogAsync(BotLogType.Common, string.Format("Auto Ban {0} <{1} ({2})> Successful! {3}#{4}", 
					SystemUtilities.GetSeparator, 
					Context.Guild.Name, 
					Context.Guild.Id, 
					user.Username, 
					user.DiscriminatorValue));
			} 
			catch (Exception e) 
			{
				await ReplyAsync("Unable to auto-ban user! Please be sure that I'm higher up on the role list.");

				var output = new StringBuilder()
					.AppendFormat("Auto Ban {0} <{1} ({2})> Failure!", 
						SystemUtilities.GetSeparator, 
						Context.Guild.Name, 
						Context.Guild.Id).AppendLine()
					.AppendFormat("---- Reason : {0}", 
						e.Message);

				await _botLog.SendBotLogAsync(BotLogType.Common, output.ToString());
			}
		}

		[Command, UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)]
		public Task ExecuteAsync(IGuildUser user) => ExecuteAsync(user, "No Reason Specified");
	}
}