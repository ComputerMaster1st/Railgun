using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Warning
{
    public partial class Warning
    {
        [Alias("empty"), UserPerms(GuildPermission.ManageGuild)]
        public class WarningEmpty : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Warning;

                if (data.Warnings.Count < 1)
                    return ReplyAsync("Warnings list is already empty.");

                data.Warnings.Clear();
                return ReplyAsync("Warnings list is now empty.");
            }
        }
    }
}
