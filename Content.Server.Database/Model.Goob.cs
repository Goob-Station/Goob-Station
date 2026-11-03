using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

public abstract partial class ServerDbContext
{
    public DbSet<AchievementProgress> AchievementProgress { get; set; } = default!;

    private static void GoobOnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AchievementProgress>()
            .HasOne(a => a.Player)
            .WithMany()
            .HasForeignKey(a => a.PlayerUserId)
            .HasPrincipalKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AchievementProgress>()
            .HasIndex(a => new { a.PlayerUserId, a.AchievementId })
            .IsUnique();
    }
}

[Table("achievement_progress")]
[Index(nameof(AchievementId))]
public sealed class AchievementProgress
{
    [Key]
    public int Id { get; set; }

    [Required, ForeignKey("Player")]
    public Guid PlayerUserId { get; set; }

    public Player Player { get; set; } = default!;

    [Required, StringLength(128)]
    public string AchievementId { get; set; } = default!;

    public int Progress { get; set; }

    public DateTime? UnlockedAt { get; set; }
}
