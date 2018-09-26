using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TreeDiagram.Models;

namespace TreeDiagram
{
    public class TreeDbSet<TEntity> : DbSet<TEntity> where TEntity : class, ITreeModel
    {
        public async Task<TEntity> GetOrCreateAsync(ulong id)
        {
            var data = await this.FirstOrDefaultAsync(find => find.Id == id);

            if (data != null) return data;
            
            data = (TEntity)Activator.CreateInstance(typeof(TEntity), id);
            await AddAsync(data);

            return data;
        }
        
        public async Task<TEntity> GetAsync(ulong id) 
            => await this.FirstOrDefaultAsync(find => find.Id == id);

        public async Task DeleteAsync(ulong id) 
            => Remove(await this.FirstOrDefaultAsync(find => find.Id == id));
    }
}