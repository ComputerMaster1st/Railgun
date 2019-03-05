using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models
{
    public abstract class TimerBase : ITreeTimer {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; } = 0;
        
        public ulong GuildId { get; }
        public DateTime TimerExpire { get; }

        protected TimerBase(ulong guildId, DateTime timerExpire) {
            GuildId = guildId;
            TimerExpire = timerExpire;
        }
    }
}