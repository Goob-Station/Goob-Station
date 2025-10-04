using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wraith.Systems.Mobs;
public sealed class RushdownSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

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
