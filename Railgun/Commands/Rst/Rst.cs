using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using TreeDiagram;

namespace Railgun.Commands.Rst
{
    [Alias("rst")]
	public partial class Rst : SystemBase
	{
		private readonly MasterConfig _config;

		public Rst(MasterConfig config)
			=> _config = config;

		[Command]
		public Task ExecuteAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Fun.Rst;

			if (!data.IsEnabled)
				return ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

			var msg = data.GetRst();

			if (string.IsNullOrEmpty(msg))
				msg = string.Format("RST is empty! Please add some stuff using {0}.", 
					Format.Code(string.Format("{0}rst add [message]",
						_config.DiscordConfig.Prefix)));

			return ReplyAsync(msg);
		}
	}
}