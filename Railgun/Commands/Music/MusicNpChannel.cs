using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicNp
        {
            [Alias("channel"), UserPerms(GuildPermission.ManageGuild)]
            public class MusicNpChannel : SystemBase
            {
                private Task SetNpChannelAsync(ServerMusic data, ITextChannel tc, bool locked = false)
                {
                    data.NowPlayingChannel = locked ? tc.Id : 0;

                    return ReplyAsync($"{Format.Bold("Now Playing")} messages are {Format.Bold(locked ? "Now" : "No Longer")} locked to #{tc.Name}.");
                }

                [Command]
                public Task ExecuteAsync(ITextChannel tc)
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    if (data.NowPlayingChannel != 0 && tc.Id == data.NowPlayingChannel)
                        return SetNpChannelAsync(data, tc);

                    return SetNpChannelAsync(data, tc, true);
                }

                [Command]
                public Task ExecuteAsync()
                    => ExecuteAsync(Context.Channel as ITextChannel);
            }
        }
    }
}
