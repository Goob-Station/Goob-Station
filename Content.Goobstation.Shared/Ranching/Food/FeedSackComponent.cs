using Content.Goobstation.Common.Ranching;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Ranching.Food;

[RegisterComponent, NetworkedComponent]
public sealed partial class FeedSackComponent : Component
{
    [DataField]
    public EntProtoId ChickenFeed = "ChickenFeedRandom";

    [DataField]
    public Color SeedColor = Color.LightYellow;
}

[Serializable, NetSerializable]
public enum SeedColor : byte
{
    Color,
}
