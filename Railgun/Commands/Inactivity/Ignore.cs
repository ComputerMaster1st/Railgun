using Finite.Commands;
using Railgun.Core.Commands;

namespace Railgun.Commands.Inactivity
{
    public partial class InactivityMonitor
    {
        [Alias("ignore")]
        public partial class Ignore : SystemBase {}
    }
}
