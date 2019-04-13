using Newtonsoft.Json;

namespace Railgun.Core.Configuration
{
    [JsonObject(MemberSerialization.Fields)]
    public class BotLogChannels
    {
        public ulong Common { get; set; }
        public ulong CommandMngr { get; set; }
        public ulong GuildMngr { get; set; }
        public ulong MusicMngr { get; set; }
        public ulong MusicPlayerActive { get; set; }
        public ulong MusicPlayerError { get; set; }
        public ulong TimerMngr { get; set; }
        public ulong AudioChord { get; set; }
        public ulong TaskSch { get; set; }
    }
}