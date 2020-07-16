using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Fun
{
    public class ServerFun
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public virtual FunBite Bites { get; set; } = null;
        public virtual FunRst Rst { get; set; } = null;
    }
}