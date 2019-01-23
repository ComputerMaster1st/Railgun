using System;
using Discord;
using Finite.Commands;

namespace Railgun.Core.Commands.Readers
{
    public abstract class DiscordTypeReader : ITypeReader
    {
        private IDiscordClient _client;

        public abstract Type SupportedType { get; }

        public abstract bool TryParse(string value, IDiscordClient client, out object result);

        public DiscordTypeReader(IDiscordClient client) => _client  = client;

        public bool TryRead(string value, out object result)
        {
            if (TryParse(value, _client, out object parseResult)) {
                result = parseResult;
                return true;
            }

            result = default;
            return false;
        }
    }
}