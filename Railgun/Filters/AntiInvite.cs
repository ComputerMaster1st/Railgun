using System.Text;
using System.Threading.Tasks;
using Discord;
using TreeDiagram;
using TreeDiagram.Interfaces;

namespace Railgun.Filters
{
    public class AntiInvite : FilterBase, IMessageFilter
    {
        public async Task<IUserMessage> FilterAsync(ITextChannel tc, IUserMessage message, IGuildUser self, TreeDiagramContext context)
        {
            var profile = context.ServerProfiles.GetOrCreateData(tc.GuildId);
            var data = profile.Filters.Urls;

            if (!CheckConditions(data as ITreeFilter, message)) return null;

            var content = message.Content.ToLower();

            if (!data.BlockServerInvites || !content.Contains("discord.gg/")) return null;

            var output = new StringBuilder()
                .AppendFormat("{0} Deleted {1}'s Message! {2}", Format.Bold("Anti-Url :"), message.Author.Mention, Format.Bold("Reason :"))
                .AppendFormat("Server Invite");

            return await tc.SendMessageAsync(output.ToString());
        }
    }
}