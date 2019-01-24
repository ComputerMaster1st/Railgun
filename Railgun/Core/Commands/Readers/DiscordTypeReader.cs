using System;
using Discord;
using Finite.Commands;

namespace Railgun.Core.Commands.Readers
{
    public abstract class DiscordTypeReader : ISystemTypeReader
    {
        public abstract Type SupportedType { get; }

        public abstract bool TryParse(string value, SystemContext context, out object result);

        public bool TryRead(string value, out object result) => throw new NotImplementedException();

        public bool TryRead(string value, ICommandContext context, out object result)
        {
            if (TryParse(value, (SystemContext)context, out object parseResult)) {
                result = parseResult;
                return true;
            }

            result = default;
            return false;
        }
    }
}