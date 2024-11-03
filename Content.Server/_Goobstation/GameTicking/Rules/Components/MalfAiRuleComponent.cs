using Content.Shared.Roles;
using Content.Shared.Store;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(MalfAiRuleSystem))]
public sealed partial class MalfAiRuleComponent : Component
{
    public readonly List<EntityUid> MalfAiMind = new();

    public readonly List<ProtoId<StoreCategoryPrototype>> StoreCategories = new()
    {
        "MalfAiDestructiveModules",
        "MalfAiUtilityModules",
        "MalfAiUpgradeModules"
    };

    public readonly List<ProtoId<EntityPrototype>> Objectives = new()
    {
        "MalfAiSurviveObjective"
    };
}
