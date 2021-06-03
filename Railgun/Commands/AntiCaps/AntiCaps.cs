using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.AntiCaps
{
    [Alias("anticaps"), UserPerms(GuildPermission.ManageMessages), BotPerms(GuildPermission.ManageMessages)]
	public partial class AntiCaps : SystemBase
	{
		[Command]
		public Task ExecuteAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Caps;

			data.IsEnabled = !data.IsEnabled;

			return ReplyAsync($"Anti-Caps is now {Format.Bold(data.IsEnabled ? "Enabled" : "Disabled")}.");
		}
	}
}