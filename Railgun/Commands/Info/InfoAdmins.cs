using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Info
{
    public partial class Info
    {
        [Alias("admins")]
        public class InfoAdmins : SystemBase
        {
            private readonly MasterConfig _config;

            public InfoAdmins(MasterConfig config)
                => _config = config;

            [Command]
            public async Task ExecuteAsync()
            {
                var guild = await Context.Client.GetGuildAsync(_config.DiscordConfig.MasterGuildId);
                var master = await guild.GetUserAsync(_config.DiscordConfig.MasterAdminId);
                var output = new StringBuilder()
                    .AppendLine(Format.Bold(string.Format("{0}#{1}", 
                        master.Username, 
                        master.DiscriminatorValue)) 
                    + " is Railgun's Master Admin.");

                foreach (var adminId in _config.DiscordConfig.OtherAdmins)
                {
                    var admin = await guild.GetUserAsync(adminId);

                    if (admin == null)
                    {
                        output.AppendFormat(Format.Bold(string.Format("{0}", adminId)));
                        break;
                    }

                    output.AppendLine(Format.Bold(string.Format("{0}#{1}", admin.Username, admin.DiscriminatorValue)));
                }

                await ReplyAsync(output.ToString());
            }
        }
    }
}
