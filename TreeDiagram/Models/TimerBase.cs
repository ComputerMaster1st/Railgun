using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Timers;

namespace TreeDiagram.Models
{
    public abstract class TimerBase : ITreeModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; internal set; } = Convert.ToUInt64(DateTime.UtcNow.Millisecond);
        
        public ulong GuildId { get; set; }
        public ulong TextChannelId { get; set; }
        public DateTime TimerExpire { get; set; }

        [NotMapped] public Timer Timer { get; set; }
    }
}