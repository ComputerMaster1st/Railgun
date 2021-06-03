using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
        [Alias("leave"), UserPerms(GuildPermission.ManageGuild)]
        public class ServerLeave : SystemBase
        {
            [Command]
            public async Task ExecuteAsync()
            {
                await ReplyAsync("My presence is no longer required. Goodbye everyone!");

                await Task.Delay(500);

                await Context.Guild.LeaveAsync();
            }
        }
    }
}
