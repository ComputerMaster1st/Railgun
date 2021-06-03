using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("threshold")]
        public partial class Threshold : SystemBase { }
    }
}
