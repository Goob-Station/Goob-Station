namespace Content.Goobstation.Common.BlockTeleport;

[ByRefEvent]
public record struct TeleportAttemptEvent(
    bool Predicted = true,
    string? Message = "teleport-blocked-message",
    bool Cancelled = false);
