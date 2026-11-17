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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChargedHeadbuttComponent, ChargedHeadbuttEvent>(OnHeadbutt);
        SubscribeLocalEvent<ChargedHeadbuttComponent, ChargedHeadbuttDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<ChargedHeadbuttComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<ChargedHeadbuttComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnHeadbutt(Entity<ChargedHeadbuttComponent> ent, ref ChargedHeadbuttEvent args)
    {
        var (uid, comp) = ent;

        if (args.Handled || comp.IsCharging)
            return;

        var doAfterEvent = new ChargedHeadbuttDoAfterEvent { TargetCoordinates = GetNetCoordinates(args.Target) };
        var doAfterArgs = new DoAfterArgs(EntityManager, uid, comp.ChargeTime, doAfterEvent, uid)
        {
            BreakOnMove = true
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        if (comp.StartPopup is not null)
        {
            _popup.PopupPredicted(Loc.GetString(comp.StartPopup.Value), uid, uid);
        }

        _audio.PlayPredicted(comp.ChargeSound, uid, uid);

        args.Handled = true;
    }

    private void OnDoAfter(Entity<ChargedHeadbuttComponent> ent, ref ChargedHeadbuttDoAfterEvent args)
    {
        var (uid, comp) = ent;

        if (args.Cancelled || args.Handled)
            return;

        comp.IsCharging = true;
        Dirty(ent);

        var selfMap = _transform.GetMapCoordinates(uid);
        var targetMap = _transform.ToMapCoordinates(GetCoordinates(args.TargetCoordinates));
        var direction = targetMap.Position - selfMap.Position;

        if (direction.Length() > comp.DashDistance)
        {
            direction = direction.Normalized() * comp.DashDistance;
        }

        _throwing.TryThrow(uid, direction, comp.DashSpeed);

        args.Handled = true;
    }

    private void OnCollide(Entity<ChargedHeadbuttComponent> ent, ref StartCollideEvent args)
    {
        var (uid, comp) = ent;

        if (!comp.IsCharging)
            return;

        var other = args.OtherEntity;

        if (HasComp<MobStateComponent>(other))
        {
            _stun.TryAddStunDuration(other, comp.TargetStun);
            _stun.TryKnockdown(other, comp.TargetKnockdown, comp.TargetDropItems);
        }

        _damageable.TryChangeDamage(other, comp.Damage, origin: uid);

        _audio.PlayPredicted(comp.ImpactSound, uid, uid);

        comp.IsCharging = false;
        Dirty(ent);
    }

    private void OnStopThrow(Entity<ChargedHeadbuttComponent> ent, ref StopThrowEvent args)
    {
        var (_, comp) = ent;

        comp.IsCharging = false;
        Dirty(ent);
    }
}
