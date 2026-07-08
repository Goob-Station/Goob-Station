using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Kit marker for the Idol slasher. Enables the lovestruck conversion mechanic:
/// damage and soul steals from this entity charm victims into devoted fans.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherIdolComponent : Component;
