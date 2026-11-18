using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.StatusIcon.Components;

/// <summary>
/// Hides all of this entity's status icons (job, antag, health, etc.) from everyone except the entity itself.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ForceHideIconsComponent : Component;
