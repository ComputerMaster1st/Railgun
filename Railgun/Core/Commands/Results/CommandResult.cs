using System;
using System.Linq;
using Finite.Commands;

namespace Railgun.Core.Commands.Results
{
    public class CommandResult : IResult
    {
        public bool IsSuccess { get; }
        public SystemContext Context { get; }
        public CommandInfo Command { get; }
        public string CommandPath { get; }
        public Exception Exception { get; }

        public CommandResult(bool success, SystemContext ctx, CommandInfo cmd, Exception ex = null) {
            IsSuccess = success;
            Context = ctx;
            Command = cmd;
            Exception = ex;

            var path = GetModulePath(Command.Module);

            if (Command.Aliases.Count > 0) path = string.Format("{0} {1}", path, Command.Aliases.ToList()[0]);

            CommandPath = path.TrimStart(' ').TrimEnd(' ');
        }

        private string GetModulePath(ModuleInfo info) {
            if (info != null) {
                var moduleAlias = GetModulePath(info.Module);

                if (info.Aliases.Count > 0)
                    return string.Format("{0} {1}", moduleAlias, info.Aliases.ToList()[0]);
                
                return string.Empty;
            } 
            
            return string.Empty;
        }
    }
}