using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.SubModels
{
    public class IgnoredChannels
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }
        public ulong ChannelId { get; private set; }

        public IgnoredChannels(ulong channelId) => ChannelId = channelId;
    }
}