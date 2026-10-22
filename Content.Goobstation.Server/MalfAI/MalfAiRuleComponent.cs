using Content.Shared.Silicons.Laws;
using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.MalfAI;

[RegisterComponent]
public sealed partial class MalfAiRuleComponent : Component
{
    [DataField]
    public ProtoId<SiliconLawsetPrototype> Lawset = "MalfAiLawset";

    public readonly List<ProtoId<StoreCategoryPrototype>> StoreCategories = new()
    {
        "MalfAi",
    };
}
