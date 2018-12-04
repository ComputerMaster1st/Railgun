using System;
using Finite.Commands;

namespace Railgun.Core.Commands
{
    public class NullTypeReaderFactory : ITypeReaderFactory
    {
        public bool TryGetTypeReader<T>(out ITypeReader<T> reader)
        {
            reader = null;
            return false;
        }

        public bool TryGetTypeReader(Type valueType, out ITypeReader reader)
        {
            reader = null;
            return false;
        }
    }
}