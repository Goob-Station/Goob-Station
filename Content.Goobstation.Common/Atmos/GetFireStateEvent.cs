namespace Content.Goobstation.Common.Atmos;

[ByRefEvent]
public record struct GetFireStateEvent(
    bool OnFire);
