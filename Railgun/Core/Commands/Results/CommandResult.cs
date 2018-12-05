using System;
using Finite.Commands;

namespace Railgun.Core.Commands.Results
{
    public class CommandResult : IResult
    {
        public bool IsSuccess { get; }
        public SystemContext Context { get; }
        public CommandInfo Command { get; }
        public Exception Exception { get; }

        public CommandResult(bool success, SystemContext ctx, CommandInfo cmd, Exception ex = null) {
            IsSuccess = success;
            Context = ctx;
            Command = cmd;
            Exception = ex;
        }
    }
}