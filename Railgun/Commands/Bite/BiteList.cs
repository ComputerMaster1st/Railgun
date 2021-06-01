using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Extensions;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Bite
{
    public partial class Bite
    {
		[Alias("list"), BotPerms(ChannelPermission.AttachFiles)]
        public class BiteList : SystemBase
		{
			private readonly MasterConfig _config;

			public BiteList(MasterConfig config)
				=> _config = config;

			[Command]
			public Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Fun.Bites;

				if (!data.IsEnabled)
					return ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");
				if (data.Bites.Count < 1)
					return ReplyAsync($"There are no bite sentences available. To add a sentence, use {Format.Code($"{_config.DiscordConfig.Prefix}bite add <sentence>")}. You will need to place {Format.Code("<biter>")} & {Format.Code("<bitee>")} in the sentence somewhere to make this work.");

				var output = new StringBuilder()
					.AppendFormat("List of Bite Sentences: ({0} Total)", data.Bites.Count).AppendLine().AppendLine();

				data.Bites.ForEach(bite => output.AppendFormat("[{0}] {1}", Format.Code(data.Bites.IndexOf(bite).ToString()), bite).AppendLine());

				if (output.Length < 1900)
					return ReplyAsync(output.ToString());

				return (Context.Channel as ITextChannel).SendStringAsFileAsync("Bites.txt", output.ToString(),
					$"List of {Format.Bold(data.Bites.Count.ToString())} Bite Sentences");
			}
		}
    }
}
