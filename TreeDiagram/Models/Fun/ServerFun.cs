using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Fun
{
    public class ServerFun
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public virtual FunBite Bites { get; private set; } = new FunBite();
        public virtual FunRst Rst { get; private set; } = new FunRst();
    }
}