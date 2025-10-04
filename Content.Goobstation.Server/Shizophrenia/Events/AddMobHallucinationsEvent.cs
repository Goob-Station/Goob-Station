using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shizophrenia;

[DataDefinition]
public sealed partial class AddMobHallucinationsEvent : EntityEventArgs
{
    [DataField]
    public string Id = "";

    [DataField]
    public List<EntProtoId> Entities = new();

    [DataField]
    public MinMax Delay = new();

    [DataField]
    public MinMax Range = new();

    [DataField]
    public MinMax SpawnCount = new();

    [DataField]
    public float Duration = -1f;
}
