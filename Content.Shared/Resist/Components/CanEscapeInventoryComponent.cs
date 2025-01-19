using Content.Shared.DoAfter;
using Robust.Shared.GameStates;

namespace Content.Shared.Resist;

[RegisterComponent, NetworkedComponent]
public sealed partial class CanEscapeInventoryComponent : Component
{
    /// <summary>
    /// Base doafter length for uncontested breakouts.
    /// </summary>
    [DataField("baseResistTime")]
    public float BaseResistTime = 5f;

    public bool IsEscaping => DoAfter != null;

    [DataField("doAfter")]
    public DoAfterId? DoAfter;

    /// <summary>
    ///     DeltaV - action to cancel inventory escape. Added dynamically.
    /// </summary>
    [DataField]
    public EntityUid? EscapeCancelAction;
}
