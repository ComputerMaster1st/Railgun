using Railgun.Core.TypeReaders;
using System;

namespace Railgun.Core.Factories
{
    public static class DiscordTypeReaderFactoryExtensions
    {
        public static DiscordTypeReaderFactory AddReader<TEntity>(this DiscordTypeReaderFactory factory) where TEntity : DiscordTypeReader
        {
            factory.TryAddReader((TEntity)Activator.CreateInstance(typeof(TEntity)));
            return factory;
        }
    }
}