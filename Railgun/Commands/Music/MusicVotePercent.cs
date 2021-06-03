using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicVote
        {
            [Alias("percent")]
            public class MusicVotePercent : SystemBase
            {
                [Command]
                public Task ExecuteAsync(int percent)
                {
                    if (percent < 10 || percent > 100)
                        return ReplyAsync("Percentage must be set between 10-100.");

                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    data.VoteSkipLimit = percent;

                    if (!data.VoteSkipEnabled) 
                        data.VoteSkipEnabled = true;

                    return ReplyAsync($"Music Vote-Skip is now {(!data.VoteSkipEnabled ? Format.Bold("enabled &") : "")} set to skip songs when {data.VoteSkipLimit}% of users have voted.");
                }
            }
        }
    }
}
