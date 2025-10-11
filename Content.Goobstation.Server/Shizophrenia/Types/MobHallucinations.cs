using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shizophrenia;

public sealed partial class MobHallucinations : BaseHallucinationsType
{
    [DataField]
    public List<EntProtoId> Entities = new();

    [DataField]
    public MinMax Range = new();

    [DataField]
    public MinMax SpawnCount = new();

    public override BaseHallucinationsEntry GetEntry()
    {
        return new MobHallucinationsEntry()
        {
            Prototypes = Entities,
            Delay = Delay,
            Range = Range,
            SpawnCount = SpawnCount
        };
    }
}
