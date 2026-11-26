using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Bongle;

public sealed partial class ChargedHeadbuttSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;

    [SubscribeLocalEvent]
    private void OnHeadbutt(Entity<ChargedHeadbuttComponent> ent, ref ChargedHeadbuttEvent args)
    {
        if (args.Handled || ent.Comp.IsCharging)
            return;

        var doAfterEvent = new ChargedHeadbuttDoAfterEvent { TargetCoordinates = GetNetCoordinates(args.Target) };
        var doAfterArgs = new DoAfterArgs(EntityManager, ent.Owner, ent.Comp.ChargeTime, doAfterEvent, ent.Owner)
        {
            BreakOnMove = true
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        if (ent.Comp.StartPopup is not null)
        {
            _popup.PopupPredicted(Loc.GetString(ent.Comp.StartPopup.Value), ent.Owner, ent.Owner);
        }

        _audio.PlayPredicted(ent.Comp.ChargeSound, ent.Owner, ent.Owner);

        if (!HasComp<SeethingComponent>(ent.Owner)) // No cooldown on headbutt while seething because idk it sounds cool
        {
            args.Handled = true;
        }
    }

    [SubscribeLocalEvent]
    private void OnDoAfter(Entity<ChargedHeadbuttComponent> ent, ref ChargedHeadbuttDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        ent.Comp.IsCharging = true;
        Dirty(ent);

        var selfMap = _transform.GetMapCoordinates(ent.Owner);
        var targetMap = _transform.ToMapCoordinates(GetCoordinates(args.TargetCoordinates));
        var direction = targetMap.Position - selfMap.Position;

        if (direction.Length() > ent.Comp.DashDistance)
        {
            direction = direction.Normalized() * ent.Comp.DashDistance;
        }

        _throwing.TryThrow(ent.Owner, direction, ent.Comp.DashSpeed);

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnCollide(Entity<ChargedHeadbuttComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.IsCharging)
            return;

        var other = args.OtherEntity;

        _stun.TryAddStunDuration(other, ent.Comp.TargetStun);
        _stun.TryKnockdown(other, ent.Comp.TargetKnockdown, ent.Comp.TargetDropItems);
        _damageable.TryChangeDamage(other, ent.Comp.Damage, origin: ent.Owner);

        _audio.PlayPredicted(ent.Comp.ImpactSound, ent.Owner, ent.Owner);

        ent.Comp.IsCharging = false;
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnStopThrow(Entity<ChargedHeadbuttComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.IsCharging = false;
        Dirty(ent);
    }
}
