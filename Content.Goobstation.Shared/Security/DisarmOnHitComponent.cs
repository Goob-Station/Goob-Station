using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Security;

[RegisterComponent, NetworkedComponent]
public sealed partial class DisarmOnHitComponent : Component
{
    /// <summary>
    /// Chance per hit target to make them drop what they hold in their active hand.
    /// </summary>
    [DataField(required: true)]
    public float Chance;

    /// <summary>
    /// If true, only the item in the target's active hand can be dropped. If false, any held item can be dropped, starting with the active hand.
    /// </summary>
    [DataField]
    public bool ActiveHandOnly = true;
}
