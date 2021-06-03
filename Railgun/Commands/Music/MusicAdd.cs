using Finite.Commands;
using Railgun.Core;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("add")]
		public partial class MusicAdd : SystemBase
		{
			[Command]
			public Task ExecuteAsync()
				=> throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
		}
	}
}