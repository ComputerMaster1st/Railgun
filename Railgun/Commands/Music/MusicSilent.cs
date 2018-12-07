using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("silent"), UserPerms(GuildPermission.ManageGuild)]
        public class MusicSilent : SystemBase
        {
            private readonly TreeDiagramContext _db;

            public MusicSilent(TreeDiagramContext db) => _db = db;

            [Command("running")]
            public async Task RunningAsync() {
                var data = await _db.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                    
                data.SilentNowPlaying = !data.SilentNowPlaying;
                    
                await ReplyAsync($"{Format.Bold(data.SilentNowPlaying ? "Engaged" : "Disengaged")} Silent Running!");
            }
            
            [Command("install")]
            public async Task InstallAsync() {
                var data = await _db.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                
                data.SilentSongProcessing = !data.SilentSongProcessing;
                
                await ReplyAsync($"{Format.Bold(data.SilentSongProcessing ? "Engaged" : "Disengaged")} Silent Installation!");
            }
        }
    }
}