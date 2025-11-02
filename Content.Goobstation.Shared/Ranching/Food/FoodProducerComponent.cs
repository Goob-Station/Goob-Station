using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Ranching.Food;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class FoodProducerComponent : Component
{
    /// <summary>
    /// Maximum amount of food the Food Producer can hold
    /// </summary>
    [DataField]
    public int MaxFood = 4;

    /// <summary>
    /// How long it takes to produce a food
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(2);

    [ViewVariables]
    public EntProtoId FeedSack = "ChickenFeedSack";

    /// <summary>
    /// Is the machine working right now?
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool IsActive;

    [DataField]
    public SoundSpecifier? GrindSound = new SoundPathSpecifier("/Audio/Machines/blender.ogg");

    [ViewVariables]
    public string StorageContainer = "storagebase";

    [ViewVariables]
    public string BeakerContainer = "beakerSlot";

    public EntityUid? Audio;
}

[Serializable, NetSerializable]
public sealed partial class FoodProducerDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
/// Raised on the food that got produced.
/// </summary>
[ByRefEvent]
public record struct FoodProducedEvent(int FoodAmount);
