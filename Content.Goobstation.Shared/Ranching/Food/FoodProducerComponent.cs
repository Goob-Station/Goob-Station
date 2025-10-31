using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Ranching.Food;

[RegisterComponent, NetworkedComponent]
public sealed partial class FoodProducerComponent : Component
{
    [DataField]
    public string StorageContainer = "storagebase";

    [DataField]
    public string BeakerContainer = "beakerSlot";

    [DataField]
    public int MaxFood = 4;

    [ViewVariables]
    public EntProtoId FeedSack = "ChickenFeedSack";
}

[Serializable, NetSerializable]
public sealed partial class FoodProducerDoAfterEvent : SimpleDoAfterEvent;
