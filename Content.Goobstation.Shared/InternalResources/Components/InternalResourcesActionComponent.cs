using Content.Goobstation.Shared.InternalResources.Data;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.InternalResources.Components;

/// <summary>
/// Component for action that need to use internal resources
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class InternalResourcesActionComponent : Component
{
    [DataField(required: true)]
    public ProtoId<InternalResourcesPrototype> ResourceProto;

    /// <summary>
    /// Used instead of the DeficitPopup in ResourceProto when not null.
    /// </summary>
    [DataField]
    public LocId? DeficitPopup;

    [DataField]
    public float UseAmount;

    [DataField]
    public float AltUseAmount;
}
