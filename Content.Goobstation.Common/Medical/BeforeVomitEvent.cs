namespace Content.Goobstation.Common.Medical;

[ByRefEvent]
public record struct BeforeVomitEvent(
    bool Cancelled);
