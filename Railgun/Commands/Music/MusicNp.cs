using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Managers;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("np")]
        public class MusicNp : SystemBase
        {
            private readonly PlayerManager _playerManager;

            public MusicNp(PlayerManager playerManager) => _playerManager = playerManager;

            private Task SetNpChannelAsync(ServerMusic data, ITextChannel tc, bool locked = false) {
                data.NowPlayingChannel = locked ? tc.Id : 0;

                return ReplyAsync($"{Format.Bold("Now Playing")} messages are {Format.Bold(locked ? "Now" : "No Longer")} locked to #{tc.Name}.");
            }
            
            [Command]
            public Task NowPlayingAsync() {
                var container = _playerManager.GetPlayer(Context.Guild.Id);
            
                if (container == null) return ReplyAsync("I'm not playing anything at this time.");
                
                var player = container.Player;
                var meta = player.GetFirstSongRequest().Metadata;
                var currentTime = DateTime.Now - player.SongStartedAt;
                var output = new StringBuilder()
                    .AppendFormat("Currently playing {0} at the moment.", Format.Bold(meta.Name)).AppendLine()
                    .AppendFormat("Url: {0} || Length: {1}/{2}", Format.Bold($"<{meta.Url}>"), 
                                  Format.Bold($"{currentTime.Minutes}:{currentTime.Seconds}"), 
                                  Format.Bold($"{meta.Length.Minutes}:{meta.Length.Seconds}"));
            
                return ReplyAsync(output.ToString());
            }
            
            [Command("channel"), UserPerms(GuildPermission.ManageGuild)]
            public async Task SetNpChannelAsync(ITextChannel tcParam = null) {
                var data = await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                var tc = tcParam ?? (ITextChannel)Context.Channel;
                    
                if (data.NowPlayingChannel != 0 && tc.Id == data.NowPlayingChannel) {
                    await SetNpChannelAsync(data, tc);

                    return;
                }
                        
                await SetNpChannelAsync(data, tc, true);
            }
        }
    }
}