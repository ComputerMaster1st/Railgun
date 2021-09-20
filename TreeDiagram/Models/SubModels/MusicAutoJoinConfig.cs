using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.SubModels
{
    public class MusicAutoJoinConfig
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public ulong VoiceChannelId { get; private set; }
        public ulong TextChannelId { get; set; }

        private List<ulong> _listenForUsers;

        public List<ulong> ListenForUsers
        {
            get
            {
                if (_listenForUsers == null) _listenForUsers = new List<ulong>();
                return _listenForUsers;
            }
            private set
            {
                _listenForUsers = value;
            }
        }

        public MusicAutoJoinConfig(ulong voiceChannelId, ulong textChannelId)
        {
            VoiceChannelId = voiceChannelId;
            TextChannelId = textChannelId;
        }

        public bool AddListeningUser(ulong userId)
        {
            if (ListenForUsers.Contains(userId)) return false;

            ListenForUsers = new List<ulong>(ListenForUsers);
            ListenForUsers.Add(userId);
            return true;
        }

        public bool RemoveListeningUser(ulong userId)
        {
            if (!ListenForUsers.Contains(userId)) return false;

            ListenForUsers = new List<ulong>(ListenForUsers);
            ListenForUsers.RemoveAll(x => x == userId);
            return true;
        }

        public void ClearListeningUser()
            => ListenForUsers = new List<ulong>();
    }
}