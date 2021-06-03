using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("whitelist")]
        public partial class Whitelist : SystemBase { }
    }
}
