using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Bite
{
    public partial class Bite
    {
        [Alias("allowdeny"), UserPerms(GuildPermission.ManageMessages)]
        public class BiteAllowDeny : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Fun.Bites;

                data.IsEnabled = !data.IsEnabled;

                return ReplyAsync($"Bites are now {(data.IsEnabled ? Format.Bold("enabled") : Format.Bold("disabled"))}!");
            }
        }
    }
}
