using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Server.Filter.IgnoreChannel
{
    public abstract class FilterBaseIgnoreChannel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ChannelId { get; private set; }

        public FilterBaseIgnoreChannel(ulong channelId) => ChannelId = channelId;
    }
}