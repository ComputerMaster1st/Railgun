using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.SubModels
{
    public class UlongUserId
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;
        public ulong UserId { get; private set; }

        public UlongUserId(ulong userId) => UserId = userId;
    }
}