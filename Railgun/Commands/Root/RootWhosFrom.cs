using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("whosfrom")]
        public class RootWhosFrom : SystemBase
        {
            [Command]
            public async Task ExecuteAsync(ulong serverId)
            {
                var guild = await Context.Client.GetGuildAsync(serverId);

                if (guild == null)
                {
                    await ReplyAsync("Can not find specified guild/server.");

                    return;
                }

                var localUsers = await Context.Guild.GetUsersAsync();
                var remoteUsers = new List<IGuildUser>();

                foreach (var localUser in localUsers)
                {
                    var remoteUser = await guild.GetUserAsync(localUser.Id);

                    if (remoteUser != null) 
                        remoteUsers.Add(remoteUser);
                }

                if (remoteUsers.Count < 1)
                {
                    await ReplyAsync($"Unable to find users who are from {Format.Bold($"{guild.Name} <{guild.Id}>")}.");

                    return;
                }

                var output = new StringBuilder()
                    .AppendFormat("There are {0} from {1}!",
                        Format.Bold($"{remoteUsers.Count} user(s)"),
                        Format.Bold($"{guild.Name} <{guild.Id}>")).AppendLine()
                    .AppendLine();

                foreach (var remoteUser in remoteUsers)
                    output.AppendFormat("{0}#{1} {2} ", 
                        remoteUser.Username, 
                        remoteUser.DiscriminatorValue, 
                        SystemUtilities.GetSeparator);

                output.Remove(output.Length - 3, 3);

                await ReplyAsync(output.ToString());
            }
        }
    }
}
