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
            [Alias("prefix")]
            public class ServerConfigPrefix : SystemBase
            {
                [Command]
                public Task ExecuteAsync([Remainder] string input)
                {
                    var profile = Context.Database.UserProfiles.GetOrCreateData(Context.Author.Id);
                    var data = profile.Globals;

                    data.Prefix = input;

                    return ReplyAsync(string.Format("Server prefix has been set! {0}!",
                        Format.Code(input + " <command>")));
                }

                [Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.UserProfiles.GetOrCreateData(Context.Author.Id);
                    var data = profile.Globals;

                    if (string.IsNullOrWhiteSpace(data.Prefix))
                        return ReplyAsync("No prefix has been specified. Please specify a prefix.");

                    data.Prefix = string.Empty;

                    return ReplyAsync("Server prefix has been removed.");
                }
            }
        }
    }
}
