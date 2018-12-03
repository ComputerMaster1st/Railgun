using Newtonsoft.Json;

namespace Railgun.Core.Logging
{
    [JsonObject(MemberSerialization.Fields)]
    public class BotLogChannels
    {
        public ulong Common { get; set; } = 0;
        public ulong CommandMngr { get; set; } = 0;
        public ulong GuildMngr { get; set; } = 0;
        public ulong MusicMngr { get; set; } = 0;
        public ulong MusicPlayer { get; set; } = 0;
        public ulong TimerMngr { get; set; } = 0;
        public ulong AudioChord { get; set; } = 0;
        public ulong TaskSch { get; set; } = 0;
    }
}