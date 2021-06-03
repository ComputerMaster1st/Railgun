using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;

namespace Railgun.Commands.Music
{
    [Alias("music", "m"), RoleLock(ModuleType.Music)]
	public partial class Music : SystemBase { }
}