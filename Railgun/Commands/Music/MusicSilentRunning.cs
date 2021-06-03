using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicSilent
        {
            [Alias("running")]
            public class MusicSilentRunning : SystemBase
            {
                [Command]
                public Task ExecuteAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    data.SilentNowPlaying = !data.SilentNowPlaying;

                    return ReplyAsync($"{Format.Bold(data.SilentNowPlaying ? "Engaged" : "Disengaged")} Silent Running!");
                }
            }
        }
    }
}
