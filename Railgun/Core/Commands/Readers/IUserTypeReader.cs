using System;
using Discord;

namespace Railgun.Core.Commands.Readers
{
    public class IUserTypeReader : DiscordTypeReader
    {
        public IUserTypeReader(IDiscordClient client) : base(client) { }

        public override Type SupportedType => typeof(IUser);

        public override bool TryParse(string value, IDiscordClient client, out object result)
        {
            if (MentionParser.TryParseUser(value, out ulong userId)) {
                result = client.GetUserAsync(userId).GetAwaiter().GetResult();
                return true;
            }

            result = default;
            return false;
        }
    }
}