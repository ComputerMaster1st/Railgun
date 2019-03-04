using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models
{
    public abstract class TimerBase : ITreeModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; internal set; } = Convert.ToUInt64(DateTime.UtcNow.Subtract(
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
        
        public ulong GuildId { get; set; }
        public ulong TextChannelId { get; set; }
        public DateTime TimerExpire { get; set; }
    }
}