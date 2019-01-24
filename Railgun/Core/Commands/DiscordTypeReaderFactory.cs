using System;
using System.Collections.Generic;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands.Readers;

namespace Railgun.Core.Commands
{
    public class DiscordTypeReaderFactory : ITypeReaderFactory {
        private Dictionary<Type, DiscordTypeReader> _readers = new Dictionary<Type, DiscordTypeReader>();

        public bool TryAddReader(DiscordTypeReader reader) => _readers.TryAdd(reader.SupportedType, reader);

        public bool TryGetTypeReader<T>(out ITypeReader<T> reader) => throw new NotImplementedException();

        public bool TryGetTypeReader(Type valueType, out ITypeReader reader) {
            if (_readers.TryGetValue(valueType, out DiscordTypeReader value)) {
                reader = value;
                return true;
            }

            reader = default;
            return false;
        }
    }
}