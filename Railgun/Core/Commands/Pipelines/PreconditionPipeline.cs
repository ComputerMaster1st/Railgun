using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Commands.Results;

namespace Railgun.Core.Commands.Pipelines
{
    public class PreconditionPipeline : IPipeline
    {
        private CommandExecutionContext _context;
        private SystemContext _ctx;

        public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next) {
            _context = context;
            _ctx = context.Context as SystemContext;

            var moduleResult = await CheckModulePreconditions(_context.Command.Module);

            if (!moduleResult.IsSuccess) return moduleResult;

            var commandResult = await CheckPreconditionsAsync(_context.Command.Attributes);

            if (!commandResult.IsSuccess) return commandResult;

            return await next();
        }

        private async Task<IResult> CheckModulePreconditions(ModuleInfo info) {
            if (info != null) {
                var result = await CheckModulePreconditions(info.Module);

                if (!result.IsSuccess) return result;
                else if (info.Attributes.Count > 0) {
                    return await CheckPreconditionsAsync(info.Attributes);
                }
            }

            return PreconditionResult.FromSuccess();
        }

        private async Task<IResult> CheckPreconditionsAsync(IReadOnlyCollection<Attribute> attributes) {
            foreach (var attribute in attributes) {
                try {
                    var precondition = attribute as IPreconditionAttribute;
                    
                    if (precondition == null) continue;

                    var result = await precondition.CheckPermissionsAsync(_ctx, _context.Command, _context.ServiceProvider);

                    if (!result.IsSuccess) return result;
                } catch { }                
            }

            return PreconditionResult.FromSuccess();
        }
    }
}