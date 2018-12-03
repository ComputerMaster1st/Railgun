using System.Collections.Generic;
using Newtonsoft.Json;
using Railgun.Core.Logging;

namespace Railgun.Core.Configuration
{
    [JsonObject(MemberSerialization.Fields)]
    public class DiscordConfig
    {
        public string Token { get; }
        public string Prefix { get; set; }
        public ulong MasterAdminId { get; }
        public ulong MasterGuildId { get; set; } = 0;
        public List<ulong> OtherAdmins { get; } = new List<ulong>();
        public BotLogChannels BotLogChannels { get; } = new BotLogChannels();

        public DiscordConfig(string token, string prefix, ulong masterAdminId) {
            Token = token;
            Prefix = prefix;
            MasterAdminId = masterAdminId;
        }

        [JsonConstructor]
        private DiscordConfig(string token, string prefix, ulong masterAdminId, ulong masterGuildId, BotLogChannels botLogChannels, List<ulong> otherAdmins) {
            Token = token;
            Prefix = prefix;
            MasterAdminId = masterAdminId;
            MasterGuildId = masterGuildId;
            BotLogChannels = botLogChannels;
            OtherAdmins = otherAdmins;
        }

        public void AssignMasterGuild(ulong guildId)
            => MasterGuildId = guildId;
        
        public bool AssignAdmin(ulong userId) {
            if (OtherAdmins.Contains(userId)) return false;

            OtherAdmins.Add(userId);
            return true;
        }

        public bool RemoveAdmin(ulong userId) {
            if (!OtherAdmins.Contains(userId)) return false;

            OtherAdmins.Remove(userId);
            return true;
        }
    }
}
        
//         Public Sub AssignBotLogChannel(channelId As ULong, botLogType As BotLogType)
//             Select botLogType
//                 Case BotLogType.Common:
//                     _BotLogChannels.Common = channelId
//                     Exit Select
//                 Case BotLogType.CommandManager:
//                     _BotLogChannels.CommandMngr = channelId
//                     Exit Select
//                 Case BotLogType.GuildManager:
//                     _BotLogChannels.GuildMngr = channelId
//                     Exit Select
//                 Case BotLogType.MusicManager:
//                     _BotLogChannels.MusicMngr = channelId
//                     Exit Select
//                 Case BotLogType.MusicPlayer:
//                     _BotLogChannels.MusicPlayer = channelId
//                     Exit Select
//                 Case BotLogType.AudioChord:
//                     _BotLogChannels.AudioChord = channelId
//                     Exit Select
//                 Case BotLogType.TaskScheduler:
//                     _BotLogChannels.TaskSch = channelId
//                     Exit Select
//                 Case BotLogType.TimerManager:
//                     _BotLogChannels.TimerMngr = channelId
//                     Exit Select
//             End Select
//         End Sub
        
//     End Class
    
// End NameSpace