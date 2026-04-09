using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Server.Destructible;
using Content.Server.Emp;
using Content.Shared.Body.Components;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Destructible;
using Content.Shared.Doors.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.PowerCell;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

/// <summary>
/// breaks own cuffs, knocks out humanoids, EMPs cyborgs, or destroys doors/containers.
/// </summary>
public sealed class BloodsuckerBrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly SharedDestructibleSystem _destructible = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerBrawlEvent>(OnBrawl);
    }

    private void OnBrawl(Entity<BloodsuckerComponent> ent, ref BloodsuckerBrawlEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp(ent, out BloodsuckerBrawlComponent? comp))
            return;

        // break own cuffs
        if (TryComp(ent.Owner, out CuffableComponent? cuffable) && cuffable.CuffedHandCount > 0)
        {
            if (!TryUseCosts(ent, comp))
                return;

            _cuffable.Uncuff(ent.Owner, ent.Owner, cuffable.LastAddedCuffs);
            args.Handled = true;
            return;
        }

        var target = args.Target;
        if (target == EntityUid.Invalid || target == ent.Owner)
            return;

        if (!TryUseCosts(ent, comp))
            return;

        // EMP a cyborg / battery-powered entity
        if (HasComp<PowerCellComponent>(target) && TryComp(target, out MobStateComponent? _))
        {
            var coords = Transform(target).MapPosition;
            _emp.EmpPulse(coords, comp.EMPRadius, comp.EMPConsumption, comp.EMPDuration);
            _audio.PlayPredicted(comp.UseSound, ent, ent);
            args.Handled = true;
            return;
        }

        // destroy a door or container
        if (HasComp<DestructibleComponent>(target) && !HasComp<BodyComponent>(target) && HasComp<ContainerManagerComponent>(target) || HasComp<DoorComponent>(target))
        {
            _destructible.DestroyEntity(target);
            _audio.PlayPredicted(comp.UseSound, ent, ent);
            args.Handled = true;
            return;
        }

        // knock out anything else
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(comp.KnockoutTime), true);
        _audio.PlayPredicted(comp.UseSound, ent, ent);
        args.Handled = true;
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerBrawlComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity), -comp.HumanityCost);

        return true;
    }
}
