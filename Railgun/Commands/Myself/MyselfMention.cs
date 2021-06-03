using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Myself
{
    public partial class Myself
    {
        [Alias("mention")]
        public class MyselfMention : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.UserProfiles.GetOrCreateData(Context.Author.Id);
                var data = profile.Globals;

                data.DisableMentions = !data.DisableMentions;

                return ReplyAsync(string.Format("Personal mentions are now {0}.",
                    data.DisableMentions ? Format.Bold("Enabled") : Format.Bold("Disabled")));
            }
        }
    }
}
