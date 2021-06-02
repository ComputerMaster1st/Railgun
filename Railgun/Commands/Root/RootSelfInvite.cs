using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("selfinvite")]
        public class RootSelfInvite : SystemBase
        {
            private readonly MasterConfig _config;

            public RootSelfInvite(MasterConfig config)
                => _config = config;

            [Command]
            public async Task ExecuteAsync(ulong id)
            {
                var guild = await Context.Client.GetGuildAsync(id);

                try
                {
                    var invites = await guild.GetInvitesAsync();
                    var output = new StringBuilder()
                        .AppendFormat("Invite for {0}", Format.Bold($"{guild.Name} ({guild.Id}")).AppendLine()
                        .AppendLine()
                        .AppendLine(invites.FirstOrDefault().Url);

                    var masterAdmin = await Context.Client.GetUserAsync(_config.DiscordConfig.MasterAdminId);
                    var dm = await masterAdmin.GetOrCreateDMChannelAsync();

                    await dm.SendMessageAsync(output.ToString());
                }
                catch 
                { 
                    await ReplyAsync($"Unable to get invites for server id {Format.Bold(id.ToString())}"); 
                }
            }
        }
    }
}
