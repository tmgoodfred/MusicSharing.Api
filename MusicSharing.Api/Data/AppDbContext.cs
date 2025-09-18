using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MusicSharing.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("MusicSharing");

        // Many-to-many: Song <-> Category
        modelBuilder.Entity<Song>()
            .HasMany(s => s.Categories)
            .WithMany(c => c.Songs);
    }
}
