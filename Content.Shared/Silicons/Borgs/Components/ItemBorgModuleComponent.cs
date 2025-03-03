using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Silicons.Borgs.Components;

/// <summary>
/// This is used for a <see cref="BorgModuleComponent"/> that provides items to the entity it's installed into.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedBorgSystem))]
public sealed partial class ItemBorgModuleComponent : Component
{
    /// <summary>
    /// The items that are provided.
    /// </summary>
    [DataField("items", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))] // Frontier: removed
    public List<string> Items = new();

    /// <summary>
    /// Frontier: The droppable items that are provided.
    /// </summary>
    [DataField]
    public List<DroppableBorgItem> DroppableItems = new();

    /// <summary>
    /// The entities from <see cref="Items"/> that were spawned.
    /// </summary>
    [DataField("providedItems")]
    public SortedDictionary<string, EntityUid> ProvidedItems = new();

    /// <summary>
    /// The entities from <see cref="Items"/> that were spawned.
    /// </summary>
    [DataField("droppableProvidedItems")]
    public SortedDictionary<string, (EntityUid, DroppableBorgItem)> DroppableProvidedItems = new();

    /// <summary>
    /// A counter that ensures a unique
    /// </summary>
    [DataField("handCounter")]
    public int HandCounter;

    /// <summary>
    /// Whether or not the items have been created and stored in <see cref="ProvidedContainer"/>
    /// </summary>
    [DataField("itemsCrated")]
    public bool ItemsCreated;

    /// <summary>
    /// A container where provided items are stored when not being used.
    /// This is helpful as it means that items retain state.
    /// </summary>
    [ViewVariables]
    public Container ProvidedContainer = default!;

    /// <summary>
    /// An ID for the container where provided items are stored when not used.
    /// </summary>
    [DataField("providedContainerId")]
    public string ProvidedContainerId = "provided_container";
}

// Frontier: droppable borg item data definitions
[DataDefinition]
public sealed partial class DroppableBorgItem
{
    [IdDataField]
    public EntProtoId ID;

    [DataField]
    public EntityWhitelist Whitelist;
}
