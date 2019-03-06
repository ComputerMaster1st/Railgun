using System;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using TreeDiagram.Models.Filter;
using TreeDiagram.Models.Fun;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.TreeTimer;
using TreeDiagram.Models.User;

namespace TreeDiagram
{
    public sealed class TreeDiagramContext : DbContext
    {
	    public DbSet<FilterCaps> FilterCapses { get; internal set; } = null;
        public DbSet<FilterUrl> FilterUrls { get; internal set; } = null;

        public DbSet<FunBite> FunBites { get; internal set; } = null;
        public DbSet<FunRst> FunRsts { get; internal set; } = null;

        public DbSet<ServerCommand> ServerCommands { get; internal set; } = null;
        public DbSet<ServerInactivity> ServerInactivities { get; internal set; } = null;
		public DbSet<ServerJoinLeave> ServerJoinLeaves { get; internal set; } = null;
		public DbSet<ServerMention> ServerMentions { get; internal set; } = null;
		public DbSet<ServerMusic> ServerMusics { get; internal set; } = null;
		public DbSet<ServerWarning> ServerWarnings { get; internal set; } = null;

	    public DbSet<TimerAssignRole> TimerAssignRoles { get; internal set; } = null;
	    public DbSet<TimerKickUser> TimerKickUsers { get; internal set; } = null;
	    public DbSet<TimerRemindMe> TimerRemindMes { get; internal set; } = null;

	    public DbSet<UserCommand> UserCommands { get; internal set; } = null;
		public DbSet<UserMention> UserMentions { get; internal set; } = null;

        public TreeDiagramContext(DbContextOptions optionsBuilder) : base(optionsBuilder) => AppContext.SetSwitch("System.Net.Http.useSocketsHttpHandler", false);

	    protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ServerMusic>(x => {
				x.Property(y => y.PlaylistId)
					.HasConversion(input => input.ToString(), output => ObjectId.Parse(output));
				x.HasMany(f => f.AllowedRoles).WithOne().OnDelete(DeleteBehavior.Cascade);
			});
			
			modelBuilder.Entity<ServerInactivity>(x =>
			{
				x.HasMany(f => f.Users).WithOne().OnDelete(DeleteBehavior.Cascade);
				x.HasMany(f => f.RoleWhitelist).WithOne().OnDelete(DeleteBehavior.Cascade);
				x.HasMany(f => f.UserWhitelist).WithOne().OnDelete(DeleteBehavior.Cascade);
			});
			
            modelBuilder.Entity<ServerWarning>().HasMany(f => f.Warnings).WithOne().OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<FilterCaps>().HasMany(f => f.IgnoredChannels).WithOne().OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<FilterUrl>().HasMany(f => f.IgnoredChannels).WithOne().OnDelete(DeleteBehavior.Cascade);
			base.OnModelCreating(modelBuilder);
		}

		public void DeleteGuildData(ulong id)
		{
			FilterCapses.DeleteData(id);
			FilterUrls.DeleteData(id);
			
			FunBites.DeleteData(id);
			FunRsts.DeleteData(id);
			
			ServerCommands.DeleteData(id);
            ServerInactivities.DeleteData(id);
            ServerMentions.DeleteData(id);
			ServerMusics.DeleteData(id);
			ServerWarnings.DeleteData(id);
			ServerJoinLeaves.DeleteData(id);

			SaveChanges();
		}

		public override void Dispose()
		{
			SaveChanges();
			base.Dispose();
		}
	}
}