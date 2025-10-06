using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wraith.Systems.Mobs;
public sealed class RushdownSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<StatusEffectsComponent> _statusEffectsQuery;
    public override void Initialize()
    {
        base.Initialize();

        _statusEffectsQuery = GetEntityQuery<StatusEffectsComponent>();

        SubscribeLocalEvent<RushdownComponent, RushdownEvent>(OnRushdown);
        SubscribeLocalEvent<RushdownComponent, StartCollideEvent>(OnCollide);

        SubscribeLocalEvent<RushdownComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<RushdownComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnCollide(Entity<RushdownComponent> ent, ref StartCollideEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;

        if (!comp.IsLeaping)
            return;

        if ((HasComp<MobStateComponent>(args.OtherEntity)))
        {
            _stun.TryKnockdown(args.OtherEntity, comp.CollideKnockdown, true);
        }
        else
        {
            _stun.TryKnockdown(uid, comp.CollideKnockdown, false);
        }

        comp.IsLeaping = false;
        Dirty(ent);
    }
    private void OnLand(Entity<RushdownComponent> ent, ref LandEvent args)
    {
        ent.Comp.IsLeaping = false;
        Dirty(ent);

        // --- AoE stun section ---
        var xform = Transform(ent);
        var position = xform.Coordinates;

        // define how far the stun AOE reaches
        var range = ent.Comp.LandShockwaveRange;

        // find nearby entities
        var nearby = _lookup.GetEntitiesInRange(ent.Owner, range);

        foreach (var target in nearby)
        {
            if (target == ent.Owner) // skip self
                continue;

            // only affect things that can be stunned
            if (!_statusEffectsQuery.TryComp(target, out var status))
                continue;

            _stun.KnockdownOrStun(target, ent.Comp.CollideKnockdown, true, status);
            _audio.PlayPredicted(ent.Comp.ShockwaveSound, ent.Owner, null); //This might be playing twice for some reason. Should be fine though.
        }
    }

    private void OnStopThrow(Entity<RushdownComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.IsLeaping = false;
        Dirty(ent);
    }
    private void OnRushdown(Entity<RushdownComponent> ent, ref RushdownEvent args)
    {
        // TODO: Add popups
        ent.Comp.IsLeaping = true;
        Dirty(ent);

        var xform = Transform(args.Performer);
        var throwing = xform.LocalRotation.ToWorldVec() * ent.Comp.JumpDistance;
        var direction = xform.Coordinates.Offset(throwing); // to make the character jump in the direction he's looking

        _throwing.TryThrow(args.Performer, direction, ent.Comp.JumpThrowSpeed);

        _audio.PlayPredicted(ent.Comp.JumpSound, args.Performer, args.Performer);

        args.Handled = true;
    }

}
