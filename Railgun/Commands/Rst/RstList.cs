using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Extensions;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Rst
{
    public partial class Rst
    {
		[Alias("list"), BotPerms(ChannelPermission.AttachFiles)]
        public class RstList : SystemBase
        {
			private readonly MasterConfig _config;

			public RstList(MasterConfig config)
				=> _config = config;

			[Command]
			public Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Fun.Rst;

				if (data.Rst.Count < 1)
					return ReplyAsync(string.Format("RST is empty! Please add some stuff using {0}.",
						Format.Code(string.Format("{0}rst add [message]",
							_config.DiscordConfig.Prefix))));

				if (!data.IsEnabled)
					return ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

				var output = new StringBuilder()
					.AppendLine("Randomly Selected Text List :").AppendLine();

				data.Rst.ForEach(msg => output.AppendFormat("[{0}] {1}", 
					Format.Code(data.Rst.IndexOf(msg).ToString()), 
					msg).AppendLine());

				if (output.Length < 1950)
					return ReplyAsync(output.ToString());

				return (Context.Channel as ITextChannel).SendStringAsFileAsync("RST.txt", output.ToString());
			}
		}
    }
}
