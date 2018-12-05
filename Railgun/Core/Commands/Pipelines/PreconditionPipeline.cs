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

            foreach (var attribute in context.Command.Attributes) {
                try {
                    var precondition = attribute as IPreconditionAttribute;
                    
                    if (precondition == null) continue;

                    result = await precondition.CheckPermissionsAsync(ctx, context.Command, context.ServiceProvider);

                    if (!result.IsSuccess) return result;
                } catch { }                
            }

            return await next();
        }
    }
}