using System.Linq;
using Discord;
using TreeDiagram.Interfaces;

namespace Railgun.Core.Filters
{
    public abstract class AntiFilterBase
    {
        protected bool CheckConditions(ITreeFilter data, IUserMessage message) {
            if (data == null || !data.IsEnabled ||
				(!data.IncludeBots && (message.Author.IsBot | message.Author.IsWebhook)) ||
				data.IgnoredChannels.Any(f => f.ChannelId == (message.Channel as ITextChannel).Id)) return false;
            return true;
        }
    }
}