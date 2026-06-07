using Content.Goobstation.Shared.InternalResources.Data;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangelingEquipmentComponent : Component
{
    /// <summary>
    /// The user of the equipment.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? User;

    /// <summary>
    /// The value that will be applied to a changeling's chemical modifier
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChemModifier;

    /// <summary>
    /// The slot required.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SlotFlags? RequiredSlot;

    /// <summary>
    /// The prototype of the resource being affected.
    /// </summary>
    [DataField]
    public ProtoId<InternalResourcesPrototype> ResourceType = "ChangelingChemicals";
}
