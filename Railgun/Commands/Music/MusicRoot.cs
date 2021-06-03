using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("root"), BotAdmin]
        public partial class MusicRoot : SystemBase { }
    }
}