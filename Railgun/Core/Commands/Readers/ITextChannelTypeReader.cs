using System;
using Discord;

namespace Railgun.Core.Commands.Readers
{
    public class ITextChannelTypeReader : DiscordTypeReader
    {
        public ITextChannelTypeReader(IDiscordClient client) : base(client) { }

        public override Type SupportedType => typeof(ITextChannel);

        public override bool TryParse(string value, IDiscordClient client, out object result)
        {
            if (MentionParser.TryParseChannel(value, out ulong id)) {
                var channel = client.GetChannelAsync(id).GetAwaiter().GetResult();
                result = (ITextChannel)channel;
                return true;
            }

            result = default;
            return false;
        }
    }
}