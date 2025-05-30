using Content.Goobstation.Shared.Changeling.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Nutrition.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition;
using Content.Shared.Tag;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Changeling;

public sealed class ChangelingOrganDigestionSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FoodComponent, BeforeFullyEatenEvent>(OnBeforeFullyEaten);
    }

    private void OnBeforeFullyEaten(EntityUid uid, FoodComponent component, BeforeFullyEatenEvent args)
    {
        if (!TryComp<ChangelingOrganDigestionComponent>(args.User, out var digestion))
            return;

        if (!_tag.HasTag(uid, digestion.DigestibleTag))
            return;

        if (TryComp<ChangelingIdentityComponent>(args.User, out var lingComp))
        {
            lingComp.Chemicals += digestion.ChemicalsPerItem;
            Dirty(args.User, lingComp);
        }
    }
}