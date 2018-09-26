using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models
{
    public abstract class ConfigBase : ITreeModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public ulong Id { get; internal set; }

        internal ConfigBase(ulong id) => Id = id;
    }
}