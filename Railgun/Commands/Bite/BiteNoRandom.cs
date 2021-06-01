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
        [Alias("norandom"), UserPerms(GuildPermission.ManageMessages)]
        public class BiteNoRandom : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var data = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);

                data.Fun.Bites.NoNameRandomness = !data.Fun.Bites.NoNameRandomness;

                return ReplyAsync($"Names used for bites are {(data.Fun.Bites.NoNameRandomness ? Format.Bold("no longer random") : Format.Bold("now random"))}!");
            }
        }
    }
}
