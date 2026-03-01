using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Boss;

[RegisterComponent, NetworkedComponent]
public sealed partial class MegafaunaDamageTriggerComponent : Component
{
    /// <summary>
    /// Whether the boss has already taken damage once.
    /// </summary>
    [ViewVariables]
    public bool Triggered;
}
