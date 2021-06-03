using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("kick")]
        public partial class Kick : SystemBase { }
    }
}
