using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
        public partial class ServerConfig
        {
            [Alias("ignoreoldmsgs")]
            public class ServerConfigIgnoreMessages : SystemBase
            {
                [Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Command;

                    data.IgnoreModifiedMessages = !data.IgnoreModifiedMessages;

                    return ReplyAsync($"I will {Format.Bold(data.IgnoreModifiedMessages ? "now" : "no longer")} ignore modified messages. This includes pinned messages from now on.");
                }
            }
        }
    }
}
