using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    [Alias("music", "m"), RoleLock(ModuleType.Music)]
	public partial class Music : SystemBase
    {
        [Command]
        public Task ExecuteAsync()
            => throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
    }
}