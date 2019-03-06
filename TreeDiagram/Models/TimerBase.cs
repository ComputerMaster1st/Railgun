using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Interfaces;

namespace TreeDiagram.Models
{
    public abstract class TimerBase : ITreeTimer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;
        
        public ulong GuildId { get; private set; }
        public DateTime TimerExpire { get; private set; }

        protected TimerBase(ulong guildId, DateTime timerExpire) {
            GuildId = guildId;
            TimerExpire = timerExpire;
        }
    }
}