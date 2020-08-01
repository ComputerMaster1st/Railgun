using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server
{
    public class ServerInactivity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public bool IsEnabled { get; set; }
        public int InactiveDaysThreshold { get; set; }
        public int KickDaysThreshold { get; set; }
        public ulong InactiveRoleId { get; set; }

        private List<ulong> _userWhiteList;
        private List<ulong> _roleWhiteList;

        public virtual List<UserActivityContainer> Users { get; private set; } = new List<UserActivityContainer>();

        public List<ulong> UserWhitelist { 
            get {
                if (_userWhiteList == null) _userWhiteList = new List<ulong>();
                return _userWhiteList;
            } private set {
                _userWhiteList = value;
            }}
        
        public List<ulong> RoleWhitelist { 
            get {
                if (_roleWhiteList == null) _roleWhiteList = new List<ulong>();
                return _roleWhiteList;
            } private set {
                _roleWhiteList = new List<ulong>();
            }}
    }
}
