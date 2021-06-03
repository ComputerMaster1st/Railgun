using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Rst
{
    public partial class Rst
    {
        [Alias("remove"), UserPerms(GuildPermission.ManageMessages)]
        public class RstRemove : SystemBase
        {
			private readonly MasterConfig _config;

			public RstRemove(MasterConfig config)
				=> _config = config;

			[Command]
			public Task ExecuteAsync(int index)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Fun.Rst;

				if (data.Rst.Count < 1)
					return ReplyAsync(string.Format("RST is empty! Please add some stuff using {0}.",
						Format.Code(string.Format("{0}rst add [message]",
							_config.DiscordConfig.Prefix))));

				if (!data.IsEnabled)
					return ReplyAsync(string.Format("RST is currently {0} on this server.",
						Format.Bold("disabled")));

				if (index < 0 || index >= data.Rst.Count)
					return ReplyAsync("The Message Id provided is out of bounds. Please recheck via RST List.");

				data.Rst.RemoveAt(index);

				return ReplyAsync("Message Removed!");
			}
		}
    }
}
