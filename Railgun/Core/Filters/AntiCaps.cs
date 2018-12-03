using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Railgun.Core.Managers;
using TreeDiagram;

namespace Railgun.Core.Filters
{
    public class AntiCaps : IMessageFilter
    {
        public AntiCaps(FilterManager manager) => manager.RegisterFilter(this);

        public async Task<IUserMessage> FilterAsync(IUserMessage message, TreeDiagramContext content) {
            if (string.IsNullOrWhiteSpace(message.Content)) return null;

            var tc = (ITextChannel)message.Channel;
            var data = await content.FilterCapses.GetAsync(tc.GuildId);

            if ((data == null || !data.IsEnabled) || 
                (!data.IncludeBots && (message.Author.IsBot | message.Author.IsWebhook)) || 
                data.IgnoredChannels.Where(f => f.ChannelId == tc.Id).Count() > 0 ||
                message.Content.Length < data.Length
            ) return null;

            var self = await tc.Guild.GetCurrentUserAsync();

            if (message.Author.Id == self.Id) return null;
            else if (!self.GetPermissions(tc).ManageMessages) {
                await tc.SendMessageAsync($"{Format.Bold("Anti-Caps : ")} Triggered but missing {Format.Bold("Manage Messages")} permission!");

                return null;
            }

            var user = await tc.Guild.GetUserAsync(message.Author.Id);
            var charCount = 0;
            var capsCount = 0;

            foreach (var c in message.Content) {
                if (char.IsLetter(c)) {
                    charCount++;

                    if (char.IsUpper(c)) capsCount++;
                }
            }

            if (capsCount < 1 || charCount < data.Length) return null;

            var percent = (capsCount / charCount) * 100.00;

            if (percent < data.Percentage) return null;

            return await tc.SendMessageAsync(string.Format("{0} Deleted {1}'s Message! ({2} Caps)", 
                Format.Bold("Anti-Caps :"), 
                user.Mention,
                Format.Bold(Math.Round(percent).ToString() + "%")));
        }
    }
}