using Content.Shared._Pirate.Weapons.Ranged.Events;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;

namespace Content.Shared._Pirate.Weapons.Ranged.Systems;

public sealed class HitscanBlockSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanBasicRaycastComponent, AttemptHitscanRaycastFiredEvent>(OnAttemptHitscanRaycastFired);
    }

    private void OnAttemptHitscanRaycastFired(Entity<HitscanBasicRaycastComponent> ent, ref AttemptHitscanRaycastFiredEvent args)
    {
        if (args.Data.HitEntity is not { } target)
            return;

        var damage = CompOrNull<HitscanBasicDamageComponent>(ent)?.Damage;
        var ev = new HitScanBlockAttemptEvent(args.Data.Shooter, args.Data.Gun, target, damage);
        RaiseLocalEvent(target, ref ev);

        if (ev.Cancelled)
            args.Cancelled = true;
    }
}
