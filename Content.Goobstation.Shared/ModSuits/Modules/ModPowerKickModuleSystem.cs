using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Actions;
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
        _popup.PopupPredicted(Loc.GetString("modsuit-power-kick-charge"), user, user);
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.ChargeTime, new ModPowerKickDoAfterEvent(GetNetCoordinates(args.Target)), ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
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

        var kicking = EnsureComp<ModPowerKickingComponent>(user);
        kicking.Module = ent;
        Dirty(user, kicking);

        _throwing.TryThrow(user,
            GetCoordinates(args.Coordinates),
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

    private ModPowerKickDoAfterEvent() { }

    public ModPowerKickDoAfterEvent(NetCoordinates coordinates)
    {
        Coordinates = coordinates;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
