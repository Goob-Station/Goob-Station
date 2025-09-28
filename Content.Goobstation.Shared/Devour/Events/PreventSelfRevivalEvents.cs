namespace Content.Goobstation.Shared.Devour.Events;

[ByRefEvent]
public record struct PreventSelfRevivalEvent(
    EntityUid Target,
    String PopupText = "self-revive-fail",
    bool Handled = false,
    bool Cancelled = false);
