using System.Linq;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAuto
        {
            [Alias("join")]
            public class MusicAutoJoin : SystemBase
            {
                [Command]
                public Task JoinAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;
                    var vc = (Context.Author as IGuildUser).VoiceChannel;

                    if (vc == null && data.AutoJoinConfigs.Count < 1)
                        return ReplyAsync("Music Auto-Join is currently disabled. Please join a voice channel and run this command again to enable it.");

                    if (vc == null && data.AutoJoinConfigs.Count > 0)
                        return ReplyAsync("Please join a voice channel to (un)set auto-join.");

                    var autoJoinConfig = data.AutoJoinConfigs.FirstOrDefault(f => f.VoiceChannelId == vc.Id);

                    if (autoJoinConfig == null) {
                        data.AutoJoinConfigs.Add(new MusicAutoJoinConfig(vc.Id, Context.Channel.Id));
                        return ReplyAsync(string.Format("Voice ({0}) & Text ({1}) channels have now been set for Auto-Join! To unset, use the command again while in the same voice & text channel.", vc.Name, Context.Channel.Name));
                    }

                    if (autoJoinConfig.TextChannelId != Context.Channel.Id)
                    {
                        autoJoinConfig.TextChannelId = Context.Channel.Id;
                        return ReplyAsync(string.Format("Text Channel ({1}) has now been set for Voice Channel ({0}).", vc.Name, Context.Channel.Name));
                    }

                    data.AutoJoinConfigs.Remove(autoJoinConfig);
                    return ReplyAsync("Removed voice & text channel from Auto-Join.");
                }

                [Command("reset")]
                public Task ResetAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;

                    data.AutoJoinConfigs.Clear();
                    return ReplyAsync("Music Auto-Join has been reset and disabled.");
                }
            }
        }
    }
}