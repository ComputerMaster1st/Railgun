using Finite.Commands;
using Railgun.Core;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("gc")]
        public class RootGc : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                return ReplyAsync("GC Forced!");
            }
        }
    }
}
