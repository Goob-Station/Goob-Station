using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shredder;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShreddableComponent : Component
{
    /// <summary>
    /// What state to switch to, used so in the future people can add shredding to more things
    /// </summary>
    [DataField]
    public string ShredderState = "shredding";
}
