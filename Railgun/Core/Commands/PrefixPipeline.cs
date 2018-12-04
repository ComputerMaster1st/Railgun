using System;
using System.Threading.Tasks;
using Finite.Commands;

namespace Railgun.Core.Commands
{
    public class PrefixPipeline : IPipeline
    {
        public Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next)
        {
            throw new NotImplementedException();
        }
    }
}