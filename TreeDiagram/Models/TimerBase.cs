using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Timers;

namespace TreeDiagram.Models
{
    public class TimerBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; internal set; }
        
        public ulong GuildId { get; set; }
        public ulong TextChannelId { get; set; }
        public DateTime TimerExpire { get; set; }

        [NotMapped] public Timer Timer { get; set; }
    }
}