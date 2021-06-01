using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Bite
{
    public partial class Bite
    {
		[Alias("remove"), UserPerms(GuildPermission.ManageMessages)]
        public class BiteRemove : SystemBase
        {
			[Command]
			public Task ExecuteAsync(int index)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Fun.Bites;

				if (data == null || data.Bites.Count < 1)
					return ReplyAsync("The list of bite sentences is already empty.");
				if (!data.IsEnabled)
					return ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");
				if (index < 0 || index >= data.Bites.Count)
					return ReplyAsync("The Message Id provided is out of bounds. Please recheck via Bite List.");

				data.Bites.RemoveAt(index);

				return ReplyAsync("Message Removed.");
			}
		}
    }
}
