using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wrestler.Components;

/// <summary>
/// This component is used to check if the person attempting to do the action is a wrestler.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WrestlerComponent : Component;
