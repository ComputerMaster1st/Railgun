using System.Threading.Tasks;
using Discord;
using TreeDiagram;

namespace Railgun.Filters
{
    public interface IMessageFilter
    {
        Task<IUserMessage> FilterAsync(ITextChannel tc, IUserMessage message, TreeDiagramContext context);
    }
}