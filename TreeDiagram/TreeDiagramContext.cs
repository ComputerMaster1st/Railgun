using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using TreeDiagram.Configuration;
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
        private readonly string _host;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _data;

        private const int Port = 5432;
        
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

        public TreeDiagramContext(PostgresConfig config)
        {
            _host = config.Hostname;
            _user = config.Username;
            _pass = config.Password;
            _data = config.Database;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Server={_host};Port={Port};Database={_data};UserId={_user};Password={_pass};");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerMusic>(x =>
            {
                x.Property(y => y.PlaylistId)
                    .HasConversion(input => input.ToString(), output => ObjectId.Parse(output));
            });
            base.OnModelCreating(modelBuilder);
        }

        public async Task DeleteGuildDataAsync(ulong id)
        {
            FilterCapses.Remove(await FilterCapses.FirstOrDefaultAsync(find => find.Id == id));
            FilterUrls.Remove(await FilterUrls.FirstOrDefaultAsync(find => find.Id == id));
            
            FunBites.Remove(await FunBites.FirstOrDefaultAsync(find => find.Id == id));
            FunRsts.Remove(await FunRsts.FirstOrDefaultAsync(find => find.Id == id));
            
            ServerCommands.Remove(await ServerCommands.FirstOrDefaultAsync(find => find.Id == id));
            ServerMentions.Remove(await ServerMentions.FirstOrDefaultAsync(find => find.Id == id));
            ServerMusics.Remove(await ServerMusics.FirstOrDefaultAsync(find => find.Id == id));
            ServerWarnings.Remove(await ServerWarnings.FirstOrDefaultAsync(find => find.Id == id));
            ServerJoinLeaves.Remove(await ServerJoinLeaves.FirstOrDefaultAsync(find => find.Id == id));

            await SaveChangesAsync();
        }
    }
}