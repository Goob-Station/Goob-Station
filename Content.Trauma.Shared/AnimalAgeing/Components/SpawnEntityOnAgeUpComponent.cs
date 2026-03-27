using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.AnimalAgeing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnEntityOnAgeUpComponent : Component
{
    [DataField]
    public List<EntProtoId> EntToSpawn;

    [DataField]
    public AnimalAgeState AgeToChangeAt = AnimalAgeState.Adult;
}
