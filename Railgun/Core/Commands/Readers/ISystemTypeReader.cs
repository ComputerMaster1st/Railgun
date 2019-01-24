using Finite.Commands;

namespace Railgun.Core.Commands.Readers
{
    public interface ISystemTypeReader : ITypeReader
    {
        bool TryRead(string value, ICommandContext context, out object result);
    }
}