namespace Content.Goobstation.Shared.AlertLevel;

[ByRefEvent]
public record struct AlertLevelSelectAttemptEvent(EntityUid Station, EntityUid Console, EntityUid User, string Level)
{
    public bool Cancelled;
}
