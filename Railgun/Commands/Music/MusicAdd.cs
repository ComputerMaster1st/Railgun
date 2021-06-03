using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("add")]
		public partial class MusicAdd : SystemBase { }
	}
}