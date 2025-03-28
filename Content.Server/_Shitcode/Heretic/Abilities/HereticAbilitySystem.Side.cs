using Content.Server.Body.Components;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    private void SubscribeSide()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticCleave>(OnCleave);
    }

    private void OnCleave(Entity<HereticComponent> ent, ref EventHereticCleave args)
    {
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        if (!args.Target.IsValid(EntityManager))
            return;

        Spawn(args.Effect, args.Target);

        var bloodQuery = GetEntityQuery<BloodstreamComponent>();

        var hasTargets = false;

        var targets = _lookup.GetEntitiesInRange<MobStateComponent>(args.Target, args.Range, LookupFlags.Dynamic);
        foreach (var (target, _) in targets)
        {
            if (target == ent.Owner || HasComp<HereticComponent>(target) || HasComp<GhoulComponent>(target))
                continue;

            hasTargets = true;

            _dmg.TryChangeDamage(target, args.Damage, true, origin: ent.Owner);

            if (!bloodQuery.TryComp(target, out var blood))
                continue;

            _blood.TryModifyBloodLevel(target, args.BloodModifyAmount, blood);
            _blood.TryModifyBleedAmount(target, blood.MaxBleedAmount, blood);
        }

        if (hasTargets)
            _aud.PlayPvs(args.Sound, args.Target);
    }
}
