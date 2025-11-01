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
    [DataField]
    public string StorageContainer = "storagebase";

    [DataField]
    public string BeakerContainer = "beakerSlot";

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

    public EntityUid? Audio;
}

[Serializable, NetSerializable]
public sealed partial class FoodProducerDoAfterEvent : SimpleDoAfterEvent;
