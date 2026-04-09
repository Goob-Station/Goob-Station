using Content.Goobstation.Common.Grab;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared._White.Grab;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

/// <summary>
/// Dash towards a target, if its alive stun them, if its a corpse, rip their heart out.
/// </summary>
public sealed class BloodsuckerLungeSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly GrabIntentSystem _grab = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerLungeEvent>(OnLunge);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerLungeDoAfterEvent>(OnLungeDoAfter);
    }
    private void OnLunge(Entity<BloodsuckerComponent> ent, ref BloodsuckerLungeEvent args)
    {
        if (!TryComp(ent, out BloodsuckerLungeComponent? comp))
            return;

        if (args.Target == EntityUid.Invalid || args.Target == ent.Owner)
            return;

        if (!TryUseCosts(ent, comp))
            return;

        comp.CurrentTarget = GetNetEntity(args.Target);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            comp.StartDelay,
            new BloodsuckerLungeDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            BreakOnMove = false,
            BreakOnDamage = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnLungeDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerLungeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (!TryComp(ent, out BloodsuckerLungeComponent? comp))
            return;

        var target = GetEntity(comp.CurrentTarget);
        if (target == EntityUid.Invalid || !Exists(target))
            return;

        if (TryComp(target, out TransformComponent? targetXform))
            _transform.SetWorldPosition(ent.Owner, targetXform.WorldPosition);

        if (TryComp(target, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead)
        {
            RipHeart(ent, target);
            return;
        }

        _grab.TryGrab(ent.Owner, target, true, GrabStage.Hard);
    }

    private void RipHeart(EntityUid vampire, EntityUid corpse)
    {
        if (!TryComp(corpse, out BodyComponent? body))
            return;

        // Try to find and remove the heart organ
        foreach (var (organ, _) in _body.GetBodyOrgans(corpse, body))
        {
            var meta = MetaData(organ);
            if (!meta.EntityPrototype?.ID.ToLowerInvariant().Contains("heart") ?? true)
                continue;

            _body.RemoveOrgan(organ);

            // Drop it at the corpse location
            if (TryComp(corpse, out TransformComponent? xform))
                _transform.SetWorldPosition(organ, xform.WorldPosition);

            break;
        }
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerLungeComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity), -comp.HumanityCost);

        return true;
    }
}
