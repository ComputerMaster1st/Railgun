using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Server
{
    public class ServerCommand
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Prefix { get; set; }
        public bool DeleteCmdAfterUse { get; set; }
        public bool RespondToBots { get; set; }
        public bool IgnoreModifiedMessages { get; set; }
    }
}