using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.SpellCards;

[RegisterComponent, NetworkedComponent]
public sealed partial class LockOnMarkActionComponent : Component
{
    [DataField]
    public float LockOnRadius = 3f;
}
