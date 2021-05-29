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

		[Command("allowdeny"), UserPerms(GuildPermission.ManageMessages)]
		public Task AllowDenyAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Fun.Rst;

			data.IsEnabled = !data.IsEnabled;
			return ReplyAsync($"RST is now {(data.IsEnabled ? Format.Bold("enabled") : Format.Bold("disabled"))}!");
		}

		[Command("import"), UserPerms(GuildPermission.ManageMessages)]
		public async Task ImportAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Fun.Rst;
			
			if (Context.Message.Attachments.Count < 1)
			{
                await ReplyAsync("Please attach the RST data file.");
                return;
            }

			var response = await ReplyAsync("Processing RST data file. Standby...");
			var importFileUrl = Context.Message.Attachments.First().Url;
            var importFileName = Context.Guild.Name + $"-rst-data{SystemUtilities.FileExtension}";

			using (var webClient = new HttpClient())
            using (var writer = File.OpenWrite(importFileName))
			{
                var importStream = await webClient.GetStreamAsync(importFileUrl);
                await importStream.CopyToAsync(writer);
            }

			var importFile = await File.ReadAllLinesAsync(importFileName);
			var rst = new StringBuilder();
			var rstCount = 0;

			foreach (var line in importFile) 
			{
				if (line.StartsWith('#')) continue;
                if (line.StartsWith(">>>")) 
				{
					rst = new StringBuilder();
					continue;
				}
				if (line.StartsWith("<<<"))
				{
					if (data.Rst.Contains(rst.ToString())) continue;

					data.Rst.Add(rst.ToString());
					rstCount++;
					continue;
				}

                rst.AppendLine(line);
            }

            File.Delete(importFileName);

			await response.ModifyAsync(x => x.Content = $"RST data file processed! Added {Format.Bold(rstCount.ToString())} RST entries!");
		}

		[Command("export"), UserPerms(GuildPermission.ManageMessages), BotPerms(ChannelPermission.AttachFiles)]
		public async Task ExportAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Fun.Rst;

			if (data.Rst.Count < 1)
			{
                await ReplyAsync("There's no RST data to export.");
                return;
            }

			var response = await ReplyAsync("Building RST data file. Standby...");
			var output = new StringBuilder()
                .AppendFormat("# {0}'s RST List.", Context.Guild.Name).AppendLine()
                .AppendFormat("# Generated At : {0}", DateTime.Now).AppendLine()
                .AppendLine("#")
                .AppendLine("# !!! DO NOT CHANGE/MODIFY THIS FILE !!! ")
                .AppendLine();

			foreach (var rst in data.Rst)
				output.AppendLine(">>>")
					.AppendLine(rst)
					.AppendLine("<<<");

            await (Context.Channel as ITextChannel).SendStringAsFileAsync($"rst-data{SystemUtilities.FileExtension}", output.ToString());
			await response.DeleteAsync();
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