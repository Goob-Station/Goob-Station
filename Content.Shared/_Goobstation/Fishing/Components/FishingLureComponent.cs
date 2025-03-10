using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FishingLureComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid FishingRod;

    [DataField, AutoNetworkedField]
    public EntityUid? FishingSpot;
}
