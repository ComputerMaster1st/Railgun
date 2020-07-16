using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.User
{
    public class UserGlobals
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Prefix { get; set; } = string.Empty;
        public bool DisableMentions { get; set; } = false;
    }
}