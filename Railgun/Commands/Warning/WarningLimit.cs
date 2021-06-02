using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Warning
{
    public partial class Warning
    {
		[Alias("limit"), UserPerms(GuildPermission.ManageGuild)]
		public class WarningLimit : SystemBase
		{
			[Command]
			public Task ExecuteAsync(int limit)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Warning;

				if (limit < 0)
					return ReplyAsync("The limit entered is invalid. Must be 0 or higher.");

				string message;

				if (limit > 0)
				{
					message = string.Format("Auto-Ban{0} warning limit is now set to {1}. You can disable warnings by changing the limit to 0.",
						data.WarnLimit == 0 ? " is now enabled and the" : "",
						Format.Bold(limit.ToString()));

					data.WarnLimit = limit;
				}
				else
				{
					data.WarnLimit = limit;
					message = "Auto-Ban has been disabled.";
				}

				return ReplyAsync(message);
			}

			[Command]
			public Task ExecuteAsync()
				=> ExecuteAsync(5);
		}
	}
}
