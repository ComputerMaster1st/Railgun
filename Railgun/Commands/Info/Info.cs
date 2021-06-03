using Finite.Commands;
using Railgun.Core;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Info
{
    [Alias("info")]
    public partial class Info : SystemBase
    {
        [Command]
        public Task ExecuteAsync()
            => throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
    }
}