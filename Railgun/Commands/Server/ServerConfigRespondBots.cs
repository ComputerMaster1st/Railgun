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
            [Alias("respondtobots")]
            public class ServerConfigRespondBots : SystemBase
            {
                [Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Command;

                    data.RespondToBots = !data.RespondToBots;

                    return ReplyAsync($"I will {Format.Bold(data.RespondToBots ? "now" : "no longer")} respond to other bots.");
                }
            }
        }
    }
}
