using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAuto
        {
            [Alias("download")]
            public class MusicAutoDownload : SystemBase
            {
                [Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    data.AutoDownload = !data.AutoDownload;

                    return ReplyAsync($"Music Auto-Download is now {Format.Bold(data.AutoDownload ? "Enabled" : "Disabled")}.");
                }
            }
        }
    }
}
