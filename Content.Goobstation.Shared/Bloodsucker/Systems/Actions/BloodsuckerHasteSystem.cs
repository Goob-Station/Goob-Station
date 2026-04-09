using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Stunnable;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodsuckerHasteSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerHasteEvent>(OnHaste);
    }

    private void OnHaste(Entity<BloodsuckerComponent> ent, ref BloodsuckerHasteEvent args)
    {
        if (!TryComp(ent, out BloodsuckerHasteComponent? comp))
            return;

        // Can't dash while being pulled
        if (TryComp(ent.Owner, out PullableComponent? pullable) && pullable.BeingPulled)
            return;

        if (!TryUseCosts(ent, comp))
            return;

        if (!TryComp(ent.Owner, out TransformComponent? xform))
            return;

        var origin = xform.WorldPosition;
        var destination = args.Target.Position; // WorldTargetActionEvent gives MapCoordinates

        // Clamp to dash range
        var dir = destination - origin;
        var dist = dir.Length();
        if (dist > comp.DashSpeed)
            destination = origin + Vector2.Normalize(dir) * comp.DashSpeed;

        // Knock down everyone in the swept area
        var min = Vector2.Min(origin, destination) - new Vector2(0.5f, 0.5f);
        var max = Vector2.Max(origin, destination) + new Vector2(0.5f, 0.5f);
        var sweepBox = new Box2(min, max);

        foreach (var hit in _lookup.GetEntitiesIntersecting(xform.MapID, sweepBox))
        {
            if (hit == ent.Owner)
                continue;

            _stun.TryKnockdown(hit, TimeSpan.FromSeconds(2f), true);
        }

        // Raise trail event for client visuals
        //var trailEv = new BloodsuckerHasteTrailEvent(origin, destination);
        //RaiseLocalEvent(ent.Owner, ref trailEv);

        _transform.SetWorldPosition(ent.Owner, destination);
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerHasteComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity), -comp.HumanityCost);

        return true;
    }
}
