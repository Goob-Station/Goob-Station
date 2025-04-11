using Content.Goobstation.Common.Administration.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Humanoid;

public sealed partial class BaldifySystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<BaldifyOnHitComponent, MeleeHitEvent>(OnBaldifyMeleeHit);
    }
    private void OnBaldifyMeleeHit(EntityUid uid, BaldifyOnHitComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.Count < 1)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<HumanoidAppearanceComponent>(hit))
                continue;

            EnsureComp<BaldifyComponent>(hit);
        }
    }
}