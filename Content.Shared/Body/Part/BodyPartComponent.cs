// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

// Shitmed Change
using Content.Shared._Shitmed.Body.Part;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Prototypes;
namespace Content.Shared.Body.Part;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
//[Access(typeof(SharedBodySystem))] // goob edit - all access :godo:
public sealed partial class BodyPartComponent : Component, ISurgeryToolComponent // Shitmed Change
{
    // Need to set this on container changes as it may be several transform parents up the hierarchy.
    /// <summary>
    /// Parent body for this part.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Body;

    // Shitmed Change Start

    [DataField, AutoNetworkedField]
    public BodyPartSlot? ParentSlot;

    [DataField]
    public string ToolName { get; set; } = "A body part";

    [DataField]
    public string SlotId = string.Empty;

    [DataField, AutoNetworkedField]
    public bool? Used { get; set; } = null;

    [DataField]
    public float Speed { get; set; } = 1f;

    /// <summary>
    ///     Shitmed Change: What composition does this body part classify as
    /// </summary>
    [DataField]
    public BodyPartComposition PartComposition = BodyPartComposition.Organic;

    /// <summary>
    ///     Shitmed Change: Whether this body part is enabled or not.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    ///     Shitmed Change: Whether this body part can be enabled or not. Used for non-functional prosthetics.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanEnable = true;

    /// <summary>
    /// Whether this body part can attach children or not.
    /// </summary>
    [DataField]
    public bool CanAttachChildren = true;

    /// <summary>
    ///     Shitmed Change: The name of the container for this body part. Used in insertion surgeries.
    /// </summary>
    [DataField]
    public string ContainerName { get; set; } = "part_slot";

    /// <summary>
    ///     Shitmed Change: The slot for item insertion.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ItemSlot ItemInsertionSlot = new();


    /// <summary>
    ///     Shitmed Change: Current species. Dictates things like body part sprites.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Species { get; set; } = "";

    /// <summary>
    ///     Shitmed Change: The ID of the base layer for this body part.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? BaseLayerId;

    /// <summary>
    ///     Shitmed Change: On what WoundableSeverity we should re-enable the part.
    /// </summary>
    [DataField, AutoNetworkedField]
    public WoundableSeverity EnableIntegrity = WoundableSeverity.Severe;

    [DataField, AutoNetworkedField]
    public BodyPartType PartType = BodyPartType.Other;

    [DataField, AutoNetworkedField]
    public BodyPartSymmetry Symmetry = BodyPartSymmetry.None;

    /// <summary>
    ///     When attached, the part will ensure these components on the entity, and delete them on removal.
    /// </summary>
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry? OnAdd;

    /// <summary>
    ///     When removed, the part will ensure these components on the entity, and add them on removal.
    /// </summary>
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry? OnRemove;

    // Shitmed Change End

    /// <summary>
    /// Child body parts attached to this body part.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, BodyPartSlot> Children = new();

    /// <summary>
    /// Organs attached to this body part.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, OrganSlot> Organs = new();

    /// <summary>
    /// These are only for VV/Debug do not use these for gameplay/systems
    /// </summary>
    [ViewVariables]
    private List<ContainerSlot> BodyPartSlotsVV
    {
        get
        {
            List<ContainerSlot> temp = new();
            var containerSystem = IoCManager.Resolve<IEntityManager>().System<SharedContainerSystem>();

            foreach (var slotId in Children.Keys)
            {
                temp.Add((ContainerSlot) containerSystem.GetContainer(Owner, SharedBodySystem.PartSlotContainerIdPrefix+slotId));
            }

            return temp;
        }
    }

    [ViewVariables]
    private List<ContainerSlot> OrganSlotsVV
    {
        get
        {
            List<ContainerSlot> temp = new();
            var containerSystem = IoCManager.Resolve<IEntityManager>().System<SharedContainerSystem>();

            foreach (var slotId in Organs.Keys)
            {
                temp.Add((ContainerSlot) containerSystem.GetContainer(Owner, SharedBodySystem.OrganSlotContainerIdPrefix+slotId));
            }

            return temp;
        }
    }
}

/// <summary>
/// Contains metadata about a body part in relation to its slot.
/// </summary>
[NetSerializable, Serializable]
[DataRecord]
public partial struct BodyPartSlot
{
    public string Id;
    public BodyPartType Type;
    public BodyPartSymmetry Symmetry; // Shitmed Change - Adds Symmetry to BodyPartSlot
    public BodyPartSlot(string id, BodyPartType type, BodyPartSymmetry symmetry)
    {
        Id = id;
        Type = type;
        Symmetry = symmetry;
    }
};

/// <summary>
/// Contains metadata about an organ part in relation to its slot.
/// </summary>
[NetSerializable, Serializable]
[DataRecord]
public partial struct OrganSlot
{
    public string Id;

    public OrganSlot(string id)
    {
        Id = id;
    }
};
