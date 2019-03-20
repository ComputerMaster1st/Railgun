using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using TreeDiagram;

namespace Railgun.Core.Filters
{
    public class AntiInvite : IMessageFilter
    {
        public Task<IUserMessage> FilterAsync(ITextChannel tc, IUserMessage message, TreeDiagramContext context)
        {
            var data = context.FilterUrls.GetData(tc.GuildId);

            if (data == null || !data.IsEnabled ||
                (!data.IncludeBots && (message.Author.IsBot | message.Author.IsWebhook)) ||
                data.IgnoredChannels.Any(f => f.ChannelId == tc.Id)) return null;

            var content = message.Content.ToLower();

            if (!data.BlockServerInvites || !content.Contains("discord.gg/")) return null;

            var output = new StringBuilder()
                .AppendFormat("{0} Deleted {1}'s Message! {2}", Format.Bold("Anti-Url :"), message.Author.Mention, Format.Bold("Reason :"))
                .AppendFormat("Server Invite");

            return tc.SendMessageAsync(output.ToString());
        }
    }
}