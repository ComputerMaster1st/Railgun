using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		public partial class MusicAdd
		{
			[Alias("youtube", "yt")]
			public partial class MusicAddYoutube : SystemBase { }
		}
	}
}