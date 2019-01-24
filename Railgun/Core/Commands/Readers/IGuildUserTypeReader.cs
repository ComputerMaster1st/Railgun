using System;
using Discord;

namespace Railgun.Core.Commands.Readers
{
    public class IGuildUserTypeReader : DiscordTypeReader
    {
        public override Type SupportedType => typeof(IGuildUser);

        public override bool TryParse(string value, SystemContext context, out object result)
        {
            if (MentionParser.TryParseUser(value, out ulong id)) {
                result = context.Guild.GetUserAsync(id).GetAwaiter().GetResult();
                return true;
            }

            result = default;
            return false;
        }
    }
}