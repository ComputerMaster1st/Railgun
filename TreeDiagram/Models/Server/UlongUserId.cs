using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Server
{
    public class UlongUserId {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; } = 0;
        public ulong UserId { get; }

        public UlongUserId(ulong userId) => UserId = userId;
    }
}