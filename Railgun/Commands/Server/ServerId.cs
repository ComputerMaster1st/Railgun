using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
        [Alias("id"), UserPerms(GuildPermission.ManageGuild)]
        public class ServerId : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
                => ReplyAsync($"This server's ID is {Format.Bold(Context.Guild.Id.ToString())}");
        }
    }
}
