using Content.Shared.Actions;

namespace Content.Goobstation.Shared.MantisBlades;

/// <summary>
/// This component serves for visuals and multihit whitelist.
/// </summary>
[RegisterComponent]
public sealed partial class MantisBladeComponent : Component
{
    public TimeSpan VisualDuration = TimeSpan.FromSeconds(0.3);
}

public sealed partial class ToggleRightMantisBladeEvent : InstantActionEvent;
public sealed partial class ToggleLeftMantisBladeEvent : InstantActionEvent;
