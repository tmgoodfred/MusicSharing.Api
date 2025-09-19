using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Follower> Followers { get; set; }
    public DbSet<Activity> Activities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("MusicSharing");

        // Many-to-many: Song <-> Category
        modelBuilder.Entity<Song>()
            .HasMany(s => s.Categories)
            .WithMany(c => c.Songs);

        // Many-to-many: Playlist <-> Song
        modelBuilder.Entity<Playlist>()
            .HasMany(p => p.Songs)
            .WithMany(s => s.Playlists);

        modelBuilder.Entity<Follower>()
            .HasOne(f => f.FollowerUser)
            .WithMany()
            .HasForeignKey(f => f.FollowerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Follower>()
            .HasOne(f => f.FollowedUser)
            .WithMany()
            .HasForeignKey(f => f.FollowedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Activity>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
