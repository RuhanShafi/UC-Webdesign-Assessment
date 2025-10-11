using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UC_Web_Assessment.Models;

namespace UC_Web_Assessment.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UC_Web_Assessment.Models.AIImage> AIImage { get; set; }
    public DbSet<UC_Web_Assessment.Models.ImageLike> ImageLike { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ImageLike relationship
        modelBuilder.Entity<ImageLike>()
            .HasOne(il => il.AIImage)
            .WithMany(ai => ai.Likes)
            .HasForeignKey(il => il.AIImageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure unique constraint: one like per user per image
        modelBuilder.Entity<ImageLike>()
            .HasIndex(il => new { il.AIImageId, il.UserId })
            .IsUnique();
    }
}