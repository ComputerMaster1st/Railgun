using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Rst
{
    public partial class Rst
    {
		[Alias("add")]
        public class RstAdd : SystemBase
        {
			[Command]
			public Task ExecuteAsync([Remainder] string msg)
			{
				if (string.IsNullOrWhiteSpace(msg))
					return ReplyAsync("Your message was empty. Please add a message to add.");

				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Fun.Rst;

				if (!data.IsEnabled)
					return ReplyAsync(string.Format("RST is currently {0} on this server.", Format.Bold("disabled")));

				data.AddRst(msg);

				return ReplyAsync(string.Format("Added To RST: {0}", Format.Code(msg)));
			}
		}
    }
}
