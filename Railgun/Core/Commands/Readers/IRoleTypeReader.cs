using System;
using Discord;

namespace Railgun.Core.Commands.Readers
{
    public class IRoleTypeReader : DiscordTypeReader
    {
        public override Type SupportedType => typeof(IRole);

        public override bool TryParse(string value, SystemContext context, out object result)
        {
            if (MentionParser.TryParseUser(value, out ulong id)) {
                result = context.Guild.GetRole(id);
                return true;
            }

            result = default;
            return false;
        }
    }
}