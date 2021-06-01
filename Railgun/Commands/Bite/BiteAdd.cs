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
		[Alias("add")]
        public class BiteAdd : SystemBase
        {
			[Command]
			public Task AddAsync([Remainder] string msg)
			{
				if (string.IsNullOrWhiteSpace(msg))
					return ReplyAsync("You didn't specify a sentence!");

				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Fun.Bites;

				if (!data.IsEnabled)
					return ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");

				data.AddBite(msg);

				return ReplyAsync($"Added Sentence: {Format.Code(msg)}");
			}
		}
    }
}
