using Finite.Commands;
using Railgun.Core;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		public partial class MusicAdd
		{
			[Alias("youtube", "yt")]
			public partial class MusicAddYoutube : SystemBase
			{
				[Command]
				public Task ExecuteAsync()
					=> throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
			}
		}
	}
}