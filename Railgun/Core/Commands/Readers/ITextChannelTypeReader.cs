using System;
using Discord;

namespace Railgun.Core.Commands.Readers
{
    public class ITextChannelTypeReader : DiscordTypeReader
    {
        public override Type SupportedType => typeof(ITextChannel);

        public override bool TryParse(string value, SystemContext context, out object result)
        {
            if (MentionParser.TryParseChannel(value, out ulong id)) {
                result = context.Guild.GetTextChannelAsync(id).GetAwaiter().GetResult();
                return true;
            }

            result = default;
            return false;
        }
    }
}