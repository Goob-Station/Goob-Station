using Content.Goobstation.Shared.Blob.Prototypes;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Blob.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobObserverControllerComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid BlobCore;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobProjectionComponent : Component
{
    [ViewVariables]
    public bool IsProcessingMoveEvent;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Core;

    public override bool SendOnlyToOwner => true;
}

[Serializable, NetSerializable]
public sealed class BlobChemSwapBoundUserInterfaceState(
    List<ProtoId<BlobChemPrototype>> chemList,
    ProtoId<BlobChemPrototype> selectedId)
    : BoundUserInterfaceState
{
    public readonly List<ProtoId<BlobChemPrototype>> ChemList = chemList;
    public readonly ProtoId<BlobChemPrototype> SelectedChem = selectedId;
}

[Serializable, NetSerializable]
public sealed class BlobChemSwapPrototypeSelectedMessage(ProtoId<BlobChemPrototype> selectedId) : BoundUserInterfaceMessage
{
    public readonly ProtoId<BlobChemPrototype> SelectedId = selectedId;
}

[Serializable, NetSerializable]
public enum BlobChemSwapUiKey : byte
{
    Key
}
