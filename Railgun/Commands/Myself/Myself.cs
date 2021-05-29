using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.Myself
{
	[Alias("myself", "self")]
	public partial class Myself : SystemBase { }
}