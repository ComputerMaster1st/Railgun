using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("serverlist"), BotPerms(ChannelPermission.AttachFiles)]
        public class RootServerList : SystemBase
        {
            [Command]
            public async Task ExecuteAsync()
            {
                var guilds = await Context.Client.GetGuildsAsync();
                var output = new StringBuilder()
                    .AppendFormat("Railgun Connected Server List: ({0} Servers Listed)", guilds.Count).AppendLine().AppendLine();

                foreach (var guild in guilds) 
                    output.AppendFormat("{0} : {1}", guild.Id, guild.Name).AppendLine();

                await (Context.Channel as ITextChannel).SendStringAsFileAsync("Connected Servers.txt", output.ToString(), $"({guilds.Count} Servers Listed)", false);
            }
        }
    }
}
