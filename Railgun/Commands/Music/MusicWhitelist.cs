using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("whitelist"), UserPerms(GuildPermission.ManageGuild)]
        public class MusicWhitelist : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Music;

                data.WhitelistMode = !data.WhitelistMode;

                return ReplyAsync($"Music Whitelist Mode is now {Format.Bold(data.WhitelistMode ? "Enabled" : "Disabled")}.");
            }
        }
    }
}
