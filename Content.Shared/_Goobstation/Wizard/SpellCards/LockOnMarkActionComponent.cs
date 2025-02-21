using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.SpellCards;

[RegisterComponent, NetworkedComponent]
public sealed partial class LockOnMarkActionComponent : Component
{
    [DataField]
    public float LockOnRadius = 3f;
}
