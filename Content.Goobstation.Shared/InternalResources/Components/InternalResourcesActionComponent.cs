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

    [DataField]
    public float UseAmount = 0;
}
