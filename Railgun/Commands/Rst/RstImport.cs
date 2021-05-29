using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Rst
{
    public partial class Rst
	{
		[Alias("import"), UserPerms(GuildPermission.ManageMessages)]
        public class RstImport : SystemBase
        {
			[Command]
			public async Task ExecuteAsync()
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
					if (line.StartsWith('#')) 
						continue;

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

				await response.ModifyAsync(x => x.Content = string.Format("RST data file processed! Added {0} RST entries!",
					Format.Bold(rstCount.ToString())));
			}
		}
    }
}
