using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server
{
	public class ServerMusic
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }
		
		public virtual List<MusicAutoJoinConfig> AutoJoinConfigs { get; private set; } = new List<MusicAutoJoinConfig>();
		public bool AutoSkip { get; set; }
		public bool AutoDownload { get; set; }
		public string AutoPlaySong { get; set; } = string.Empty;
		public bool PlaylistAutoLoop { get; set; } = true;
		public ObjectId PlaylistId { get; set; } = ObjectId.Empty;
		public bool VoteSkipEnabled { get; set; }
		public int VoteSkipLimit { get; set; } = 50;
		public bool SilentNowPlaying { get; set; }
		public bool SilentSongProcessing { get; set; }
		public ulong NowPlayingChannel { get; set; }
		public bool DisableShuffle { get; set; }

		private List<ulong> _allowedRoles;

		public List<ulong> AllowedRoles { 
			get {
				if (_allowedRoles == null) _allowedRoles = new List<ulong>();
				return _allowedRoles;
			} private set {
				_allowedRoles = value;
			}}

		public bool WhitelistMode { get; set; }

		public bool AddAllowedRole(ulong roleId)
        {
            if (AllowedRoles.Contains(roleId)) return false;

            AllowedRoles = new List<ulong>(AllowedRoles);
            AllowedRoles.Add(roleId);
            return true;
        }

        public bool RemoveAllowedRole(ulong roleId)
        {
            if (!AllowedRoles.Contains(roleId)) return false;

            AllowedRoles = new List<ulong>(AllowedRoles);
            AllowedRoles.RemoveAll(x => x == roleId);
            return true;            
        }
	}
}