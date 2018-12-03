using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Railgun.Core.Managers;
using TreeDiagram;
using TreeDiagram.Models.Server.Filter;

namespace Railgun.Core.Filters
{
    public class AntiUrl : IMessageFilter
    {
        private readonly Regex _regex = new Regex("(http(s)?)://(www.)?");

        public AntiUrl(FilterManager manager) => manager.RegisterFilter(this);

        private bool CheckContentForUrl(FilterUrl data, string content) {
            foreach (var url in data.BannedUrls) {
                if (data.DenyMode && !content.Contains(url) && _regex.IsMatch(content)) return true;
                else if (!data.DenyMode && content.Contains(url)) return true;
            }

            return false;
        }

        public async Task<IUserMessage> FilterAsync(IUserMessage message, TreeDiagramContext context) {
            if (string.IsNullOrWhiteSpace(message.Content)) return null;

            var tc = (ITextChannel)message.Channel;
            var data = await context.FilterUrls.GetAsync(tc.GuildId);

            if (data == null || !data.IsEnabled || 
                (!data.IncludeBots && (message.Author.IsBot | message.Author.IsWebhook)) || 
                data.IgnoredChannels.Where(f => f.ChannelId == tc.Id).Count() > 0
            ) return null;

            var self = await tc.Guild.GetCurrentUserAsync();
            var user = message.Author;

            if (message.Author.Id == self.Id) return null;
            else if (!self.GetPermissions(tc).ManageMessages) {
                await tc.SendMessageAsync($"{Format.Bold("Anti-Url :")} Triggered but missing {Format.Bold("Manage Messages")} permission!");
                
                return null;
            }

            var content = message.Content.ToLower();
            var output =  new StringBuilder()
                .AppendFormat("{0} Deleted {1}'s Message! {2}", Format.Bold("Anti-Url :"), user.Mention, Format.Bold("Reason :"));

            if (data.BlockServerInvites && content.Contains("discord.gg/")) {
                output.AppendFormat("Server Invite");

                return await tc.SendMessageAsync(output.ToString());
            } else if (_regex.IsMatch(content) && CheckContentForUrl(data, content)) {
                output.AppendFormat("Unlisted Url Block");

                return await tc.SendMessageAsync(output.ToString());
            } else if (CheckContentForUrl(data, content)) {
                output.AppendFormat("Listed Url Block");

                return await tc.SendMessageAsync(output.ToString());
            }

            return null;
        }
    }
}