using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;

namespace Railgun.Commands.Root
{
    public partial class Root 
    {
        [Alias("admin")]
        public partial class RootAdmin : SystemBase { }
    }
}