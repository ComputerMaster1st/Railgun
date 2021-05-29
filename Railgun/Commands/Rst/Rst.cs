using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Extensions;
using TreeDiagram;

namespace Railgun.Commands.Rst
{
    [Alias("rst")]
	public partial class Rst : SystemBase
	{
		private readonly MasterConfig _config;

		public Rst(MasterConfig config) => _config = config;

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

		[Command("reset"), UserPerms(GuildPermission.ManageMessages)]
		public Task ResetAsync()
		{
			var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync("RST has no data to reset.");

			data.Fun.ResetRst();
			return ReplyAsync("RST has been reset.");
		}
	}
}