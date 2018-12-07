using System;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands.Results;

namespace Railgun.Core.Commands.Attributes
{
    public interface IPreconditionAttribute
    {
        Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services);
    }
}