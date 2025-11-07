using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Werewolf.Components;


[RegisterComponent]
public sealed partial class WerewolfRuleComponent : Component
{
    public readonly List<EntityUid> WerewolfMinds = new();

    public readonly List<ProtoId<StoreCategoryPrototype>> StoreCategories = new() // todo, also yes i am making this based on a changeling stfu
    {
        "WerewolfMutate"
    };
}
