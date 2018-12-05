using System;
using System.Threading.Tasks;
using Finite.Commands;

namespace Railgun.Core.Commands.Pipelines
{
    public class PreconditionPipeline : IPipeline
    {
        public Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next) {
            var attributes = context.Command.Attributes;

            return null;
        }
    }
}