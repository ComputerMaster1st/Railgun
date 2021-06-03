using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.JoinLeave
{
    [Alias("joinleave", "jl"), UserPerms(GuildPermission.ManageGuild)]
	public partial class JoinLeave : SystemBase
	{
		[Command]
		public Task ExecuteAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.JoinLeave;

			if (data.ChannelId == Context.Channel.Id) 
			{
				data.ChannelId = 0;
				return ReplyAsync($"Join/Leave Notifications is now {Format.Bold("Disabled")}.");
			}

			data.ChannelId = Context.Channel.Id;

			return ReplyAsync($"Join/Leave Notifications is now {Format.Bold((data.ChannelId == 0 ? "Enabled & Set" : "Set"))} to this channel.");
		}
	}
}