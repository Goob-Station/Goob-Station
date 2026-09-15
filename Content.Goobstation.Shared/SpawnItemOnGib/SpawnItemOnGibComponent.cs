using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpawnItemOnGib;

[RegisterComponent]
public sealed partial class SpawnItemsOnGibComponent : Component
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> ItemsToSpawn;
}
