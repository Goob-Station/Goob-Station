using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpawnItemOnGib;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpawnItemsOnGibComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public Dictionary<EntProtoId, int> ItemsToSpawn;
}
