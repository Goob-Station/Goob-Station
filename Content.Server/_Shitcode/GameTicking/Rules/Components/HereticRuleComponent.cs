using Content.Shared.Destructible.Thresholds;
using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(HereticRuleSystem))]
public sealed partial class HereticRuleComponent : Component
{
    [DataField]
    public MinMax RealityShiftPerHeretic = new(3, 4);

    [DataField]
    public EntProtoId RealityShift = "EldritchInfluence";

    public readonly List<EntityUid> Minds = new();

    public readonly List<ProtoId<StoreCategoryPrototype>> StoreCategories = new()
    {
        "HereticPathAsh",
        //"HereticPathLock",
        "HereticPathFlesh",
        "HereticPathBlade",
        "HereticPathVoid",
        "HereticPathRust",
        "HereticPathSide",
    };
}
