using System;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands.Myself
{
    [Alias("myself", "self")]
	public partial class Myself : SystemBase
	{
		[Command]
		public Task ExecuteAsync()
			=> throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
	}
}