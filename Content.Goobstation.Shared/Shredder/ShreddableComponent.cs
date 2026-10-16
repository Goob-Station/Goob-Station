using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shredder;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShreddableComponent : Component
{
    /// <summary>
    /// What state to switch to, used so multiple items can have unique shredding sprites.
    /// </summary>
    [DataField]
    public string ShredderState = "shredding";
}
