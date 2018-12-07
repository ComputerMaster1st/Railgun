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
        [Alias("auto"), UserPerms(GuildPermission.ManageGuild)]
        public class MusicAuto : SystemBase
        {
            [Command("join")]
            public async Task JoinAsync() {
                var data = await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                var vc = ((IGuildUser)Context.Author).VoiceChannel;
                    
                if (vc == null && data.AutoVoiceChannel == 0) {
                    await ReplyAsync("Music Auto-Join is currently disabled. Please join a voice channel and run this command again to enable it.");

                    return;
                } else if (vc == null && data.AutoVoiceChannel != 0) {
                    data.AutoVoiceChannel = 0;
                    data.AutoTextChannel = 0;
                        
                    await ReplyAsync("Music Auto-Join has been disabled.");

                    return;
                }
                    
                data.AutoVoiceChannel = vc.Id;
                data.AutoTextChannel = Context.Channel.Id;
                    
                await ReplyAsync($"{(data.AutoVoiceChannel == 0 ? "Music Auto-Join is now enabled!" : "")} Will automatically join {Format.Bold(vc.Name)} and use {Format.Bold("#" + Context.Channel.Name)} to post status messages.");
            }
            
            [Command("skip")]
            public async Task SkipAsync() {
                var data = await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                
                data.AutoSkip = !data.AutoSkip;
                
                await ReplyAsync($"Music Auto-Skip is now {Format.Bold(data.AutoSkip ? "Enabled" : "Disabled")}.");
            }
            
            [Command("download")]
            public async Task DownloadAsync() {
                var data = await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id);
                
                data.AutoDownload = !data.AutoDownload;
                
                await ReplyAsync($"Music Auto-Download is now {Format.Bold(data.AutoDownload ? "Enabled" : "Disabled")}.");
            }
        }
    }
}