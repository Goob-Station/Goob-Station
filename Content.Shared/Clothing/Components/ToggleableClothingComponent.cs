using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Clothing.Components;

// GOOBSTATION - MODSUITS FULLY CHANGE THIS SYSTEM

/// <summary>
///     This component gives an item an action that will equip or un-equip some clothing e.g. hardsuits and hardsuit helmets.
/// </summary>
[Access(typeof(ToggleableClothingSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToggleableClothingComponent : Component
{
    public const string DefaultClothingContainerId = "toggleable-clothing";

    /// <summary>
    ///     Action used to toggle the clothing on or off.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "ActionToggleSuitPiece";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    // Goobstation - ClothingPrototype and Slot Fields saved for compatibility with old prototype
    /// <summary>
    ///     Default clothing entity prototype to spawn into the clothing container.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId? ClothingPrototype;

    /// <summary>
    ///     The inventory slot that the clothing is equipped to.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public string Slot = string.Empty;

    /// <summary>
    ///     Dictionary of inventory slots and entity prototypes to spawn into the clothing container.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, EntProtoId> ClothingPrototypes = new();

    /// <summary>
    ///     Dictionary of clothing uids and slots
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<EntityUid, string> ClothingUids = new();

    /// <summary>
    ///     The inventory slot flags required for this component to function.
    /// </summary>
    [DataField("requiredSlot"), AutoNetworkedField]
    public SlotFlags RequiredFlags = SlotFlags.OUTERCLOTHING;

    /// <summary>
    ///     The container that the clothing is stored in when not equipped.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ContainerId = DefaultClothingContainerId;

    [ViewVariables]
    public Container? Container;

    /// <summary>
    ///     Time it takes for this clothing to be toggled via the stripping menu verbs. Null prevents the verb from even showing up.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? StripDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Text shown in the toggle-clothing verb. Defaults to using the name of the <see cref="ActionEntity"/> action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? VerbText;

    /// <summary>
    ///     If true it will block unequip of this entity until all attached clothing are removed
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BlockUnequipWhenAttached = false;

    /// <summary>
    ///     If true all attached will replace already equipped clothing on equip attempt
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ReplaceCurrentClothing = false;
}

[Serializable, NetSerializable]
public enum ToggleClothingUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ToggleableClothingUiMessage : BoundUserInterfaceMessage
{
    public NetEntity AttachedClothingUid;

    public ToggleableClothingUiMessage(NetEntity attachedClothingUid)
    {
        AttachedClothingUid = attachedClothingUid;
    }
}
