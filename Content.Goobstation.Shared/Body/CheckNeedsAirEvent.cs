namespace Content.Goobstation.Shared.Body;

[ByRefEvent]
public record struct CheckNeedsAirEvent(
    bool Cancelled);
