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
        [Alias("vote"), UserPerms(GuildPermission.ManageGuild)]
        public class MusicVote : SystemBase
        {
            [Command]
            public async Task EnableAsync() {
                var data = await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                
                data.VoteSkipEnabled = !data.VoteSkipEnabled;
                
                await ReplyAsync($"Music Vote-Skip is now {(data.VoteSkipEnabled ? Format.Bold($"Enabled @ {data.VoteSkipLimit}%") : Format.Bold("Disabled"))}.");
            }
            
            [Command("percent")]
            public async Task SetPercentAsync(int percent) {
                if (percent < 10 || percent > 100) {
                    await ReplyAsync("Percentage must be set between 10-100.");

                    return;
                }
                
                var data = await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                
                data.VoteSkipLimit = percent;
                
                if (!data.VoteSkipEnabled) data.VoteSkipEnabled = true;
                
                await ReplyAsync($"Music Vote-Skip is now {(!data.VoteSkipEnabled ? Format.Bold("enabled &") : "")} set to skip songs when {data.VoteSkipLimit}% of users have voted.");
            }
        }
    }
}