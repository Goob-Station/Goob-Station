namespace Content.Goobstation.Common.CorticalBorer;

[ByRefEvent]
public record struct CheckCorticalBorerEvent(bool Found = false);


[ByRefEvent]
public readonly record struct EjectCorticalBorerEvent;
