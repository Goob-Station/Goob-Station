using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Gravity;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModPowerKickModuleSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedGravitySystem _gravity = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private ThrowingSystem _throwing = default!;

    [SubscribeLocalEvent]
    private void OnKickAction(Entity<ModPowerKickModuleComponent> ent, ref ModPowerKickActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        var user = args.Performer;
        if (IsWeightless(user) || !_modSuit.TryStartModuleUse(ent, user))
            return;

        _audio.PlayPredicted(ent.Comp.ChargeSound, user, user);
        _popup.PopupPredicted(Loc.GetString(ent.Comp.ChargePopup), user, user);
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.ChargeTime, new ModPowerKickDoAfterEvent(GetNetCoordinates(args.Target), GetNetEntity(args.Entity), CompOrNull<TargetActionComponent>(args.Action)?.Range), ent)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
        });
    }

    [SubscribeLocalEvent]
    private void OnKickDoAfter(Entity<ModPowerKickModuleComponent> ent, ref ModPowerKickDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;
        var user = args.User;
        if (IsWeightless(user))
            return;

        _audio.PlayPredicted(ent.Comp.LaunchSound, user, user);

        var kicking = EnsureComp<ModPowerKickingComponent>(user);
        kicking.Module = ent;
        Dirty(user, kicking);

        var target = GetEntity(args.Target) is { } targetEnt && !TerminatingOrDeleted(targetEnt)
            ? Transform(targetEnt).Coordinates
            : GetCoordinates(args.Coordinates);

        if (args.Range is { } range)
        {
            var from = _transform.GetMapCoordinates(user);
            var to = _transform.ToMapCoordinates(target);
            var offset = to.Position - from.Position;
            if (to.MapId == from.MapId && offset.Length() > range)
                target = _transform.ToCoordinates(new MapCoordinates(from.Position + offset.Normalized() * range, from.MapId));
        }

        _throwing.TryThrow(user,
            target,
            ent.Comp.ThrowSpeed,
            user,
            pushbackRatio: 0f,
            recoil: false,
            animated: false,
            playSound: false,
            doSpin: false);
    }

    private bool IsWeightless(EntityUid user)
    {
        if (!_gravity.IsWeightless(user))
            return false;

        _popup.PopupClient(Loc.GetString("modsuit-power-kick-weightless"), user, user);
        return true;
    }

    [SubscribeLocalEvent]
    private void OnKickHit(Entity<ModPowerKickingComponent> ent, ref ThrowDoHitEvent args)
    {
        if (ent.Comp.Module is not { } module || !TryComp<ModPowerKickModuleComponent>(module, out var kick))
        {
            RemCompDeferred<ModPowerKickingComponent>(ent);
            return;
        }

        _damageable.TryChangeDamage(args.Target, kick.Damage, origin: ent);
        if (HasComp<MobStateComponent>(args.Target))
            _stun.TryKnockdown(args.Target, kick.KnockdownTime);

        _audio.PlayPredicted(kick.HitSound, args.Target, ent);
        RemCompDeferred<ModPowerKickingComponent>(ent);
    }

    [SubscribeLocalEvent]
    private void OnKickStop(Entity<ModPowerKickingComponent> ent, ref StopThrowEvent args)
    {
        RemCompDeferred<ModPowerKickingComponent>(ent);
    }
}

public sealed partial class ModPowerKickActionEvent : WorldTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class ModPowerKickDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public NetCoordinates Coordinates { get; private set; }

    [DataField]
    public NetEntity? Target { get; private set; }

    [DataField]
    public float? Range { get; private set; }

    private ModPowerKickDoAfterEvent() { }

    public ModPowerKickDoAfterEvent(NetCoordinates coordinates, NetEntity? target, float? range)
    {
        Coordinates = coordinates;
        Target = target;
        Range = range;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
