using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.AnimalAgeing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnEntityOnOldAgeDeathComponent : Component
{
    [DataField]
    public List<EntProtoId> EntToSpawn;

    [DataField]
    public float RequiredHappiness;

    [DataField]
    public bool GreaterThan = true;
}
