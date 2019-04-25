using System;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Results;

namespace Railgun.Core.Attributes
{
    public interface IPreconditionAttribute
    {
        Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services);
    }
}