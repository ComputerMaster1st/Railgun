using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Filter
{
    public class ServerFilters
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public virtual FilterCaps Caps { get; private set; } = new FilterCaps();
        public virtual FilterUrl Urls { get; private set; } = new FilterUrl();

        public void ResetCaps() => Caps = new FilterCaps();
        
        public void ResetUrls() => Urls = new FilterUrl();
    }
}