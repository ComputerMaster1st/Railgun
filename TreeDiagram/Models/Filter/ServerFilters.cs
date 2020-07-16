using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Filter
{
    public class ServerFilters
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public virtual FilterCaps Caps { get; set; } = null;
        public virtual FilterUrl Urls { get; set; } = null;
    }
}