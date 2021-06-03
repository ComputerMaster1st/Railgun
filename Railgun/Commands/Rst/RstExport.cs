using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Rst
{
    public partial class Rst
    {
        [Alias("export"), UserPerms(GuildPermission.ManageMessages), BotPerms(ChannelPermission.AttachFiles)]
        public class RstExport : SystemBase
        {
            [Command]
            public async Task ExecuteAsync()
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
        }
    }
}
