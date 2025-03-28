using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

[Table("achievements")]
[PrimaryKey(nameof(PlayerId), nameof(AchievementId))]
[Index(nameof(PlayerId))]
[Index(nameof(AchievementId))]
public sealed class Achievement
{
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public string AchievementId { get; set; } = string.Empty;

    public float Progress { get; set; }

    public DateTime LastUpdatedAt { get; set; }

}
