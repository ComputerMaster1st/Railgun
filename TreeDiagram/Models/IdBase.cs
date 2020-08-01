using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Interfaces;

namespace TreeDiagram.Models
{
    public abstract class IdBase : ITreeModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public ulong Id { get; private set; }

        internal IdBase(ulong id) => Id = id;
    }
}