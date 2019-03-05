using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models
{
    public abstract class TimerBase : ITreeTimer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; internal set; }
        
        public ulong GuildId { get; set; }
        public ulong TextChannelId { get; set; }
        public DateTime TimerExpire { get; set; }
    }
}