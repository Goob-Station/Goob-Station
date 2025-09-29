using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Flash.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Storage;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Shared.Wraith.Systems.Mobs;
public sealed partial class RushdownSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
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
    }
    private void OnLand(Entity<RushdownComponent> ent, ref LandEvent args)
    {
        ent.Comp.IsLeaping = false;
    }

    private void OnStopThrow(Entity<RushdownComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.IsLeaping = false;
    }
    private void OnRushdown(Entity<RushdownComponent> ent, ref RushdownEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        comp.IsLeaping = true;
        var xform = Transform(args.Performer);
        var throwing = xform.LocalRotation.ToWorldVec() * comp.JumpDistance;
        var direction = xform.Coordinates.Offset(throwing); // to make the character jump in the direction he's looking

        _throwing.TryThrow(args.Performer, direction, comp.JumpThrowSpeed);

        _audio.PlayPredicted(comp.JumpSound, args.Performer, args.Performer);

        args.Handled = true;
    }

}
