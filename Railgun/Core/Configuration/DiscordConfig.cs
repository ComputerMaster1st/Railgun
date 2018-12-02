using System.Collections.Generic;
using Newtonsoft.Json;

namespace Railgun.Core.Configuration
{
    [JsonObject(MemberSerialization.Fields)]
    public class DiscordConfig
    {
        public string Token { get; }
        public string Prefix { get; set; }
        public ulong MasterAdminId { get; set; } = 0;
        public ulong MasterGuildId { get; set; } = 0;
        public List<ulong> OtherAdmins { get; } = new List<ulong>();
        // public BotLogChannels BotLogChannels { get; } = new BotLogChannels();
    }
}
        
//         Public Sub New(token As String, prefix As String, masterAdminId As ULong)
//             Me.Token = token
//             Me.Prefix = prefix
//             Me.MasterAdminId = masterAdminId
//         End Sub
        
//         <JsonConstructor>
//         Private Sub New(token As String, prefix As String, masterAdminId As ULong, masterGuildId As ULong, 
//                         botLogChannels As BotLogChannels, otherAdmins As List(Of ULong))
//             Me.Token = token
//             Me.Prefix = prefix
//             Me.MasterAdminId = masterAdminId
//             Me.MasterGuildId = masterGuildId
//             Me.BotLogChannels = botLogChannels
//             Me.OtherAdmins = otherAdmins
//         End Sub
        
//         Public Sub AssignMasterGuild(guildId As ULong)
//             _MasterGuildId = guildId
//         End Sub
        
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
        
//         Public Function AssignAdmin(userId As ULong) As Boolean
//             If _OtherAdmins.Contains(userId) Then Return False
            
//             _OtherAdmins.Add(userId)
//             Return True
//         End Function
        
//         Public Function RemoveAdmin(userId As ULong) As Boolean
//             If Not (_OtherAdmins.Contains(userId)) Then Return False
            
//             _OtherAdmins.Remove(userId)
//             Return True
//         End Function
        
//     End Class
    
// End NameSpace