using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Goobstation.Shared.Augments;

/// <summary>
/// Component that allows an augment to have a user interface, opened with an action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentActivatableUIComponent : Component
{
    [DataField(required: true, customTypeSerializer: typeof(EnumSerializer))]
    public Enum? Key;
}
