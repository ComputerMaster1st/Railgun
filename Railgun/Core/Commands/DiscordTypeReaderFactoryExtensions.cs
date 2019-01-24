using Railgun.Core.Commands.Readers;

namespace Railgun.Core.Commands
{
    public static class DiscordTypeReaderFactoryExtensions
    {
        public static DiscordTypeReaderFactory AddReader(this DiscordTypeReaderFactory factory, DiscordTypeReader reader) {
            factory.TryAddReader(reader);
            
            return factory;
        }
    }
}