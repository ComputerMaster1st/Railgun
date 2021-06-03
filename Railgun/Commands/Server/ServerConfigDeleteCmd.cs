using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
        public partial class ServerConfig
        {
            [Alias("deletecmd"), BotPerms(GuildPermission.ManageMessages)]
            public class ServerConfigDeleteCmd : SystemBase
            {
                [Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Command;

                    data.DeleteCmdAfterUse = !data.DeleteCmdAfterUse;

                    return ReplyAsync($"Commands used will {Format.Bold(data.DeleteCmdAfterUse ? "now" : "no longer")} be deleted.");
                }
            }
        }
    }
}
