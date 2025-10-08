using Content.Goobstation.Shared.Retractable.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Retractable.Components;

/// <summary>
/// Used for storing an unremovable clothing within an action and summoning it in selected slot on use.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(RetractableClothingSystem))]
public sealed partial class RetractableClothingActionComponent : Component
{
    /// <summary>
    /// The slot and clothing  that will appear be spawned by the action.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, EntProtoId> SpawnedPrototypes;

    /// <summary>
    /// Sound collection to play when the item is summoned.
    /// </summary>
    [DataField]
    public SoundCollectionSpecifier? SummonSounds;

    /// <summary>
    /// Sound collection to play when the summoned item is retracted back into the action.
    /// </summary>
    [DataField]
    public SoundCollectionSpecifier? RetractSounds;

    /// <summary>
    /// The slot and clothing managed by the action. Will be summoned and hidden as the action is used.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Dictionary<string, EntityUid> ActionClothingUid = new();

    /// <summary>
    /// The container ID used to store the item.
    /// </summary>
    [DataField]
    public string ContainerId = "item-action-item-container";
}
