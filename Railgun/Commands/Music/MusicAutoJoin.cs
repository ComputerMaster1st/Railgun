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
                public async Task JoinAsync()
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Music;
                    var userVc = (Context.Author as IGuildUser).VoiceChannel;

                    if (userVc == null && data.AutoJoinConfigs.Count < 1) {
                        await ReplyAsync("Music Auto-Join is currently disabled. Please join a voice channel and run this command again to enable it.");
                        return;
                    }

                    if (userVc == null && data.AutoJoinConfigs.Count > 0) {
                        await ReplyAsync("Please join a voice channel to (un)set auto-join.");
                        return;
                    }

                    var guildVc = await Context.Guild.GetVoiceChannelAsync(userVc.Id);
                    var autoJoinConfig = data.AutoJoinConfigs.FirstOrDefault(f => f.VoiceChannelId == guildVc.Id);

                    if (autoJoinConfig == null) {
                        data.AutoJoinConfigs.Add(new MusicAutoJoinConfig(guildVc.Id, Context.Channel.Id));
                        await ReplyAsync(string.Format("Voice ({0}) & Text ({1}) channels have now been set for Auto-Join! To unset, use the command again while in the same voice & text channel.", guildVc.Name, Context.Channel.Name));
                        return;
                    }

                    if (autoJoinConfig.TextChannelId != Context.Channel.Id)
                    {
                        autoJoinConfig.TextChannelId = Context.Channel.Id;
                        await ReplyAsync(string.Format("Text Channel ({1}) has now been set for Voice Channel ({0}).", guildVc.Name, Context.Channel.Name));
                        return;
                    }

                    data.AutoJoinConfigs.Remove(autoJoinConfig);
                    await ReplyAsync("Removed voice & text channel from Auto-Join.");
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