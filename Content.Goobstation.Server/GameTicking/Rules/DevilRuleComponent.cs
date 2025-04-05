using Content.Goobstation.Server.Changeling.GameTicking.Rules;
using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.GameTicking.Rules;

[RegisterComponent, Access(typeof(DevilRuleSystem))]
public sealed partial class DevilRuleComponent : Component
{
    public readonly List<EntityUid> DevilMinds = new();

    public readonly List<ProtoId<StoreCategoryPrototype>> StoreCategories = new()
    {
        "DevilAbilityUtility",
    };
}
