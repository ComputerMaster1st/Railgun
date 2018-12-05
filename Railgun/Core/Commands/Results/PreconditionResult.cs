using Finite.Commands;

namespace Railgun.Core.Commands.Results
{
    public class PreconditionResult : IResult
    {
        public bool IsSuccess { get; private set; }
        public string Error { get; private set; }

        public static PreconditionResult FromSuccess() 
            => new PreconditionResult() { IsSuccess = true };
        
        public static PreconditionResult FromError(string error)
            => new PreconditionResult() { IsSuccess = false, Error = error };
    }
}