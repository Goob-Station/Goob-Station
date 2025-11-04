using Content.Shared.Nutrition;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Ranching.Food;

public sealed class RanchingFoodSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RanchingFoodComponent, AfterFullyEatenEvent>(OnAfterFullyEaten);
    }

    private void OnAfterFullyEaten(Entity<RanchingFoodComponent> ent, ref AfterFullyEatenEvent args)
    {
        if (!_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.User))
            return;

        var ev = new RanchingFoodEatenEvent(ent.Owner);
        RaiseLocalEvent(args.User, ref ev);
    }
}
