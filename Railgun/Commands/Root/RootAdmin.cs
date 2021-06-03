using System;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands.Root
{
    public partial class Root 
    {
        [Alias("admin")]
        public partial class RootAdmin : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
                => throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
        }
    }
}