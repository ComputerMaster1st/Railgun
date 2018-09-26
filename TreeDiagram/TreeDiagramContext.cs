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
    public class TreeDiagramContext : DbContext
    {
        private readonly string _host;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _data;

        private const int Port = 5432;
        
        public TreeDbSet<FilterCaps> FilterCapses { get; internal set; }
        public TreeDbSet<FilterUrl> FilterUrls { get; internal set; }
        
        public TreeDbSet<FunBite> FunBites { get; internal set; }
        public TreeDbSet<FunRst> FunRsts { get; internal set; }

        public TreeDbSet<ServerCommand> ServerCommands { get; internal set; }
        public TreeDbSet<ServerJoinLeave> ServerJoinLeaves { get; internal set; }
        public TreeDbSet<ServerMention> ServerMentions { get; internal set; }
        public TreeDbSet<ServerMusic> ServerMusics { get; internal set; }
        public TreeDbSet<ServerWarning> ServerWarnings { get; internal set; }
        
        public TreeDbSet<TimerRemindMe> TimerRemindMes { get; internal set; }

        public TreeDbSet<UserCommand> UserCommands { get; internal set; }
        public TreeDbSet<UserMention> UserMentions { get; internal set; }

        internal TreeDiagramContext(PostgresConfig config)
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
                x.Property(y => y.PlaylistId).HasConversion(input => input.ToString(), output => ObjectId.Parse(output));
            });
            base.OnModelCreating(modelBuilder);
        }

        public async Task DeleteGuildDataAsync(ulong id)
        {
            await FilterCapses.DeleteAsync(id);
            await FilterUrls.DeleteAsync(id);
            
            await FunBites.DeleteAsync(id);
            await FunRsts.DeleteAsync(id);
            
            await ServerCommands.DeleteAsync(id);
            await ServerMentions.DeleteAsync(id);
            await ServerMusics.DeleteAsync(id);
            await ServerWarnings.DeleteAsync(id);
            await ServerJoinLeaves.DeleteAsync(id);

            await SaveChangesAsync();
        }
    }
}