using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models
{
    public abstract class TimerBase : ITreeTimer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public double Id { get; internal set; } = DateTime.UtcNow.Subtract(
            new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        
        public ulong GuildId { get; set; }
        public ulong TextChannelId { get; set; }
        public DateTime TimerExpire { get; set; }
    }
}