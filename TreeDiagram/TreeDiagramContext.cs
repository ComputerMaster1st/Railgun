using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.Server.Filter;
using TreeDiagram.Models.Server.Fun;
using TreeDiagram.Models.Server.Warning;
using TreeDiagram.Models.TreeTimer;
using TreeDiagram.Models.User;

namespace TreeDiagram
{
    public sealed class TreeDiagramContext : DbContext 
    {
        public DbSet<FilterCaps> FilterCapses { get; internal set; }
        public DbSet<FilterUrl> FilterUrls { get; internal set; }
        
        public DbSet<FunBite> FunBites { get; internal set; }
        public DbSet<FunRst> FunRsts { get; internal set; }

        public DbSet<ServerCommand> ServerCommands { get; internal set; }
        public DbSet<ServerJoinLeave> ServerJoinLeaves { get; internal set; }
        public DbSet<ServerMention> ServerMentions { get; internal set; }
        public DbSet<ServerMusic> ServerMusics { get; internal set; }
        public DbSet<ServerWarning> ServerWarnings { get; internal set; }
        
        public DbSet<TimerRemindMe> TimerRemindMes { get; internal set; }

        public DbSet<UserCommand> UserCommands { get; internal set; }
        public DbSet<UserMention> UserMentions { get; internal set; }

        public TreeDiagramContext(DbContextOptions optionsBuilder) : base(optionsBuilder) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerMusic>(x =>
            {
                x.Property(y => y.PlaylistId)
                    .HasConversion(input => input.ToString(), output => ObjectId.Parse(output));
                x.HasMany(f => f.AllowedRoles).WithOne().OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ServerWarning>().HasMany(f => f.Warnings).WithOne().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FilterCaps>().HasMany(f => f.IgnoredChannels).WithOne().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FilterUrl>().HasMany(f => f.IgnoredChannels).WithOne().OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(modelBuilder);
        }

        public async Task DeleteGuildDataAsync(ulong id)
        {
            var filterCaps = await FilterCapses.FirstOrDefaultAsync(find => find.Id == id);
            var filterUrl = await FilterUrls.FirstOrDefaultAsync(find => find.Id == id);
            var funBites = await FunBites.FirstOrDefaultAsync(find => find.Id == id);
            var funRst = await FunRsts.FirstOrDefaultAsync(find => find.Id == id);
            var serverCommand = await ServerCommands.FirstOrDefaultAsync(find => find.Id == id);
            var serverMention = await ServerMentions.FirstOrDefaultAsync(find => find.Id == id);
            var serverMusic = await ServerMusics.FirstOrDefaultAsync(find => find.Id == id);
            var serverWarning = await ServerWarnings.FirstOrDefaultAsync(find => find.Id == id);
            var serverJoinLeave = await ServerJoinLeaves.FirstOrDefaultAsync(find => find.Id == id);
            
            if (filterCaps != null) FilterCapses.Remove(filterCaps);
            if (filterUrl != null) FilterUrls.Remove(filterUrl);
            if (funBites != null) FunBites.Remove(funBites);
            if (funRst != null) FunRsts.Remove(funRst);
            if (serverCommand != null) ServerCommands.Remove(serverCommand);
            if (serverMention != null) ServerMentions.Remove(serverMention);
            if (serverMusic != null) ServerMusics.Remove(serverMusic);
            if (serverWarning != null) ServerWarnings.Remove(serverWarning);
            if (serverJoinLeave != null) ServerJoinLeaves.Remove(serverJoinLeave);

            await SaveChangesAsync();
        }

        public override void Dispose()
        {
            SaveChanges();
            base.Dispose();
        }
    }
}