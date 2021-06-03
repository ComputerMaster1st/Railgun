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
		[Alias("clear"), UserPerms(GuildPermission.BanMembers)]
        public class WarningClear : SystemBase
        {
			[Command]
			public Task ExecuteAsync(IUser user)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Warning;

				if (data.Warnings.Count < 1)
					return ReplyAsync(string.Format("There are no warnings currently issued to {0}.", user.Mention));

				var warnings = data.GetWarnings(user.Id);

				if (warnings == null || warnings.Count < 1)
					return ReplyAsync(string.Format("There are no warnings currently issued to {0}.", user.Mention));

				data.ResetWarnings(user.Id);

				return ReplyAsync(string.Format("{0} no longer has any warnings.", user.Mention));
			}
		}
    }
}
