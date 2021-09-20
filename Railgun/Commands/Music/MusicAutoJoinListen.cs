using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicAuto
        {
            public partial class MusicAutoJoin
            {
                [Alias("listen")]
                public class MusicAutoJoinListen : SystemBase
                {
                    [Command]
                    public async Task ListenAsync(IGuildUser user)
                    {
                        var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                        var data = profile.Music;
                        var userVc = (Context.Author as IGuildUser).VoiceChannel;

                        if (userVc == null)
                        {
                            await ReplyAsync("Please join a voice channel that is configured for Auto-Join.");
                            return;
                        }

                        var autoJoinConfig = data.AutoJoinConfigs.FirstOrDefault(f => f.VoiceChannelId == userVc.Id);

                        if (autoJoinConfig == null)
                        {
                            await ReplyAsync("This voice channel is not configured for auto-join. Please configure auto-join first before adding users for listening.");
                            return;
                        }

                        var output = new StringBuilder();

                        if (autoJoinConfig.ListenForUsers.Contains(user.Id))
                        {
                            autoJoinConfig.ListenForUsers.Remove(user.Id);

                            output.AppendFormat("Removed {0} from Auto-Join Listener for this voice channel.", Format.Bold(user.Username)).AppendLine();

                            if (autoJoinConfig.ListenForUsers.Count == 0)
                                output.AppendFormat("As Auto-Join Listeners is now empty, all users will now trigger Auto-Join.");
                        }
                        else
                        {
                            autoJoinConfig.ListenForUsers.Add(user.Id);

                            output.AppendFormat("Added {0} to Auto-Join Listener for this voice channel.", Format.Bold(user.Username)).AppendLine();

                            if (autoJoinConfig.ListenForUsers.Count == 1)
                                output.AppendFormat("Auto-Join Listeners will now only let users who are listed/added to trigger Auto-Join.");
                        }

                        await ReplyAsync(output.ToString());
                    }

                    [Command("clear")]
                    public async Task ClearAsync()
                    {
                        var profile = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

                        if (profile == null)
                        {
                            await ReplyAsync("Music is not yet configured on this server.");
                            return;
                        }

                        var userVc = (Context.Author as IGuildUser).VoiceChannel;

                        if (userVc == null)
                        {
                            await ReplyAsync("Please join a voice channel that is configured for Auto-Join.");
                            return;
                        }

                        var data = profile.Music;
                        var autoJoinConfig = data.AutoJoinConfigs.FirstOrDefault(f => f.VoiceChannelId == userVc.Id);

                        if (autoJoinConfig == null)
                        {
                            await ReplyAsync("Auto-Join is not configured for this channel.");
                            return;
                        }

                        autoJoinConfig.ListenForUsers.Clear();

                        await ReplyAsync("Auto-Join Listeners is now cleared. All users will now trigger Auto-Join.");
                    }
                }
            }
        }
    }
}
