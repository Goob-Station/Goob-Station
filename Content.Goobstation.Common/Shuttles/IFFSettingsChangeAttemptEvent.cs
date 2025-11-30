namespace Content.Goobstation.Common.Shuttles;

[ByRefEvent]
public record struct IFFSettingsChangeAttemptEvent(bool CanChange = true);
