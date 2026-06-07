namespace Content.Goobstation.Common.Stunnable;

[ByRefEvent]
public record struct BeforeStunEvent(
    bool Cancelled);

[ByRefEvent]
public record struct BeforeKnockdownEvent(
    bool Cancelled);

[ByRefEvent]
public record struct BeforeTrySlowdownEvent(
    bool Cancelled);
