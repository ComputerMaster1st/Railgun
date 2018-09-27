using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TreeDiagram.Models;

namespace TreeDiagram
{
    public static class DbSetExtensions
    {
        public static async Task<TEntity> CreateAsync<TEntity>(this DbSet<TEntity> set) 
            where TEntity : class, ITreeModel 
        {
            var data = (TEntity)Activator.CreateInstance(typeof(TEntity));
            await set.AddAsync(data);

            return data;
        }
        
        public static async Task<TEntity> GetOrCreateAsync<TEntity>(this DbSet<TEntity> set, ulong id)
            where TEntity : class, ITreeModel
        {
            var data = await set.FirstOrDefaultAsync(find => find.Id == id);

            if (data != null) return data;
            
            data = (TEntity)Activator.CreateInstance(typeof(TEntity), id);
            await set.AddAsync(data);

            return data;
        }

        public static async Task<TEntity> GetAsync<TEntity>(this DbSet<TEntity> set, ulong id)
            where TEntity : class, ITreeModel => await set.FirstOrDefaultAsync(find => find.Id == id);
    }
}