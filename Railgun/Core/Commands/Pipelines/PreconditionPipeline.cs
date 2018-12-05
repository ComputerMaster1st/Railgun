using System;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Commands.Results;

namespace Railgun.Core.Commands.Pipelines
{
    public class PreconditionPipeline : IPipeline
    {
        public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next) {
            var ctx = context.Context as SystemContext;
            PreconditionResult result;

            foreach (IPreconditionAttribute precondition in context.Command.Attributes) {
                if (precondition == null) continue;

                result = await precondition.CheckPermissionsAsync(ctx, context.Command, context.ServiceProvider);

                if (!result.IsSuccess) return result;
            }

            return await next();
        }
    }
}