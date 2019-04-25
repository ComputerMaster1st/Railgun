using Finite.Commands;

namespace Railgun.Core.TypeReaders
{
    public interface ISystemTypeReader : ITypeReader
    {
        bool TryRead(string value, ICommandContext context, out object result);
    }
}