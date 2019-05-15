using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using Railgun.Music;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("root"), BotAdmin]
        public class MusicRoot : SystemBase
        {
            private readonly PlayerController _playerController;

            public MusicRoot(PlayerController playerController) => _playerController = playerController;
            
            [Command("active"), BotPerms(ChannelPermission.AttachFiles)]
            public Task ActiveAsync() {
                if (_playerController.PlayerContainers.Count < 1)
                    return ReplyAsync("There are no active music streams at this time.");
                
                var output = new StringBuilder()
                    .AppendFormat("Active Music Streams ({0} Total):", _playerController.PlayerContainers.Count).AppendLine().AppendLine();
                
                foreach (var info in _playerController.PlayerContainers) {
                    var player = info.Player;
                    var song = player.GetFirstSongRequest();
                    
                    output.AppendFormat("Id : {0} {1} Spawned At : {2} {1} Status : {3}", info.GuildId, SystemUtilities.GetSeparator, player.CreatedAt, player.Status).AppendLine()
                        .AppendFormat("\\--> Latency : {0}ms {1} Playing : {2} {1} Since : {3}", player.Latency, SystemUtilities.GetSeparator, song == null ? "Searching..." : song.Id.ToString(), player.SongStartedAt).AppendLine().AppendLine();
                }
                
                if (output.Length < 1950)
                    return ReplyAsync(Format.Code(output.ToString()));
                return (Context.Channel as ITextChannel).SendStringAsFileAsync("ActivePlayers.txt", output.ToString(), includeGuildName:false);
            }
            
            [Command("kill")]
            public Task KillAsync(ulong id) {
                _playerController.DisconnectPlayer(id);
                return ReplyAsync($"Sent 'Kill Code' to Player ID {id}.");
            }
        }
    }
}