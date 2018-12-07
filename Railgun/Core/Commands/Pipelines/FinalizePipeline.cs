using System;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands.Results;

namespace Railgun.Core.Commands.Pipelines
{
    public class FinalizePipeline : IPipeline
    {
        public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next) {
            var ctx = context.Context as SystemContext;
            var cmd = context.Command;

            try { 
                await next(); 

                return new CommandResult(true, ctx, cmd);
            } 
            catch (Exception e) { return new CommandResult(false, ctx, cmd, e); }
        }
    }
}