using Content.Goobstation.Common.Ranching;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ranching.Food;

[RegisterComponent, NetworkedComponent]
public sealed partial class FeedSackComponent : Component
{
    [DataField]
    public EntProtoId ChickenFeed = "ChickenFeedRandom";
}
