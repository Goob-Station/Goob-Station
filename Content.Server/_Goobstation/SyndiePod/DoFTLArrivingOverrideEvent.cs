namespace Content.Server._Goobstation.DropPod;

[ByRefEvent]
public record struct DoFTLArrivingOverrideEvent(bool Handled = false, bool PlaySound = true, bool Cancelled = false);
