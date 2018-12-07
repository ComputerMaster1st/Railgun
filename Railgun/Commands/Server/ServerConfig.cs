using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using TreeDiagram;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
        [Alias("config"), UserPerms(GuildPermission.ManageGuild)]
        public class ServerConfig : SystemBase
        {
            [Command("mention")]
            public async Task MentionAsync() {
                var data = await Context.Database.ServerMentions.GetAsync(Context.Guild.Id);
            
                if (data != null) {
                    Context.Database.ServerMentions.Remove(data);

                    await ReplyAsync($"Server mentions are now {Format.Bold("Enabled")}.");
                    
                    return;
                }
            
                data = await Context.Database.ServerMentions.GetOrCreateAsync(Context.Guild.Id);
                data.DisableMentions = true;
            
                await ReplyAsync($"Server mentions are now {Format.Bold("Disabled")}.");
            }
        
            [Command("prefix")]
            public async Task PrefixAsync([Remainder] string input = null) {
                var data = await Context.Database.ServerCommands.GetOrCreateAsync(Context.Guild.Id);
            
                if (string.IsNullOrWhiteSpace(input) && string.IsNullOrEmpty(data.Prefix)) {
                    await ReplyAsync("No prefix has been specified. Please specify a prefix.");

                    return;
                } else if (string.IsNullOrWhiteSpace(input) && !string.IsNullOrEmpty(data.Prefix)) {
                    data.Prefix = string.Empty;

                    await ReplyAsync("Server prefix has been removed.");

                    return;
                }
            
                data.Prefix = input;
            
                await ReplyAsync($"Server prefix has been set! {Format.Code(input = "<command>")}!");
            }
        
            [Command("deletecmd"), BotPerms(GuildPermission.ManageMessages)]
            public async Task DeleteCmdAsync() {
                var data = await Context.Database.ServerCommands.GetOrCreateAsync(Context.Guild.Id);
            
                data.DeleteCmdAfterUse = !data.DeleteCmdAfterUse;
            
                await ReplyAsync($"Commands used will {Format.Bold(data.DeleteCmdAfterUse ? "now" : "no longer")} be deleted.");
            }
        
            [Command("respondtobots")]
            public async Task RespondAsync() {
                var data = await Context.Database.ServerCommands.GetOrCreateAsync(Context.Guild.Id);
            
                data.RespondToBots = !data.RespondToBots;
            
                await ReplyAsync($"I will {Format.Bold(data.RespondToBots ? "now" : "no longer")} respond to other bots.");
            }
            
            [Command("show")]
            public async Task ShowAsync() {
                var command = await Context.Database.ServerCommands.GetAsync(Context.Guild.Id);
                var mention = await Context.Database.ServerMentions.GetAsync(Context.Guild.Id);
                var output = new StringBuilder()
                    .AppendLine("Railgun Server Configuration").AppendLine()
                    .AppendFormat("    Server Name : {0}", Context.Guild.Name).AppendLine()
                    .AppendFormat("      Server ID : {0}", Context.Guild.Id).AppendLine().AppendLine()
                    .AppendFormat("     Delete CMD : {0}", command != null && command.DeleteCmdAfterUse ? "Yes": "No").AppendLine()
                    .AppendFormat("Respond To Bots : {0}", command != null && command.RespondToBots ? "Yes" : "No").AppendLine()
                    .AppendFormat("  Allow Mention : {0}", mention != null && mention.DisableMentions ? "No" : "Yes").AppendLine()
                    .AppendFormat("  Server Prefix : {0}", command != null && !string.IsNullOrEmpty(command.Prefix) ?command.Prefix : "Not Set").AppendLine();

                await ReplyAsync(Format.Code(output.ToString()));
            }
        }
    }
}