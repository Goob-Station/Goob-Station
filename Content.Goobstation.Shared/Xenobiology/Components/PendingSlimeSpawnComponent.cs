using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PendingSlimeSpawnComponent : Component
{
    [DataField] public EntProtoId BasePrototype = "MobSlimeXenobioBaby";
    [DataField] public ProtoId<BreedPrototype> Breed = "GreyMutation";
}
