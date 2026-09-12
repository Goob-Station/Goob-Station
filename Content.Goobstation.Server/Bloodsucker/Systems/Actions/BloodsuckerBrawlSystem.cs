using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Server.Emp;
using Content.Server.Storage.EntitySystems;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerBrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerBrawlEvent>(OnBrawl);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerBrawlDoAfterEvent>(OnBrawlDoAfter);
    }

    private void OnBrawl(Entity<BloodsuckerComponent> ent, ref BloodsuckerBrawlEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp(ent, out BloodsuckerBrawlComponent? comp))
            return;

        if (!TryUseCosts(ent, comp))
            return;

        var target = args.Target;
        var didSomething = false;

        // Break restraints
        if (TryBreakRestraints(ent, comp))
        {
            didSomething = true;
            // Below level CombineLevel we stop here, one thing per use.
            if (comp.ActionLevel < comp.CombineLevel)
            {
                args.Handled = true;
                return;
            }
        }

        // Escape grab
        if (TryEscapeGrab(ent, comp))
        {
            args.Handled = true;
            return;
        }

        // If we already broke restraints and escaped, done.
        if (didSomething)
        {
            args.Handled = true;
            return;
        }

        // Targeted action
        if (target == EntityUid.Invalid || target == ent.Owner)
            return;

        // Locker bash (level 3+, do-after)
        if (HasComp<EntityStorageComponent>(target) && !HasComp<DoorComponent>(target))
        {
            if (comp.ActionLevel < comp.LockerLevel)
            {
                _popup.PopupEntity(
                    Loc.GetString("bloodsucker-brawl-fail-level",
                        ("needed", comp.LockerLevel - comp.ActionLevel)),
                    ent.Owner, ent.Owner, PopupType.Small);
                return;
            }

            _popup.PopupEntity(Loc.GetString("bloodsucker-brawl-locker-start", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            _audio.PlayPvs(comp.BashSound, ent.Owner);

            StartBashDoAfter(ent, target, comp, isDoor: false);
            args.Handled = true;
            return;
        }

        // Door bash (level 4+, do-after)
        if (HasComp<DoorComponent>(target))
        {
            if (comp.ActionLevel < comp.DoorLevel)
            {
                _popup.PopupEntity(
                    Loc.GetString("bloodsucker-brawl-fail-level",
                        ("needed", comp.DoorLevel - comp.ActionLevel)),
                    ent.Owner, ent.Owner, PopupType.Small);
                return;
            }

            _popup.PopupEntity(Loc.GetString("bloodsucker-brawl-door-start", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            _audio.PlayPvs(comp.DoorPrySound, ent.Owner);

            StartBashDoAfter(ent, target, comp, isDoor: true);
            args.Handled = true;
            return;
        }

        // Mob punch
        if (HasComp<MobStateComponent>(target))
        {
            PerformPunch(ent, target, comp);
            args.Handled = true;
        }
    }

    private void OnBrawlDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerBrawlDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not EntityUid target)
            return;

        if (!TryComp(ent, out BloodsuckerBrawlComponent? comp))
            return;

        args.Handled = true;

        if (args.UserData is true) // door
        {
            if (!TryComp(target, out DoorComponent? door))
                return;

            _door.StartOpening(target);
            _popup.PopupEntity(Loc.GetString("bloodsucker-brawl-door-break", ("target", target)), target);
            _audio.PlayPvs(comp.BashSound, target);
            // Brief stun on the vampire from the impact
            _stun.TryAddStunDuration(ent.Owner, TimeSpan.FromSeconds(comp.DoorBashStun));
        }
        else // locker
        {
            if (!TryComp(target, out EntityStorageComponent? storage))
                return;

            _entityStorage.TryOpenStorage(ent.Owner, target);
            _popup.PopupEntity(Loc.GetString("bloodsucker-brawl-locker-break", ("target", target)), target);
            _audio.PlayPvs(comp.BashSound, target);
            // Force it open
            var openEv = new StorageOpenAttemptEvent(ent.Owner, Silent: false);
            RaiseLocalEvent(target, ref openEv);
        }
    }

    #region Helpers

    private bool TryBreakRestraints(Entity<BloodsuckerComponent> ent, BloodsuckerBrawlComponent comp)
    {
        if (!TryComp(ent.Owner, out CuffableComponent? cuffable) || cuffable.CuffedHandCount == 0)
            return false;

        _cuffable.Uncuff(ent.Owner, ent.Owner, cuffable.LastAddedCuffs);
        _audio.PlayPvs(comp.RestraintBreakSound, ent.Owner);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-brawl-break-restraints-others", ("user", ent.Owner)),
            Loc.GetString("bloodsucker-brawl-break-restraints-user"),
            ent.Owner, ent.Owner, PopupType.Medium);

        return true;
    }

    private bool TryEscapeGrab(Entity<BloodsuckerComponent> ent, BloodsuckerBrawlComponent comp)
    {
        if (!TryComp(ent.Owner, out PullableComponent? pullable) || pullable.Puller == null)
            return false;

        var puller = pullable.Puller.Value;

        _pulling.TryStopPull(ent.Owner, pullable);

        _stun.TryKnockdown(puller,
            TimeSpan.FromSeconds(comp.GrabEscapeKnockdown), true);

        _audio.PlayPvs(comp.GrabEscapeSound, puller);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-brawl-escape-grab-others", ("user", ent.Owner), ("puller", puller)),
            Loc.GetString("bloodsucker-brawl-escape-grab-user", ("puller", puller)),
            ent.Owner, ent.Owner, PopupType.Medium);

        return true;
    }

    private void PerformPunch(Entity<BloodsuckerComponent> ent, EntityUid target, BloodsuckerBrawlComponent comp)
    {
        var damage = comp.PunchBaseDamage + comp.ActionLevel * comp.PunchDamagePerLevel;

        var damageSpec = new DamageSpecifier();
        damageSpec.DamageDict["Blunt"] = damage;

        _damageable.TryChangeDamage(target, damageSpec, ignoreResistances: false,
            origin: ent.Owner, interruptsDoAfters: false);

        // Random knockdown chance scaling with level
        var powerLevel = Math.Min(5, 1 + comp.ActionLevel);
        if (_random.Next(5 + powerLevel) >= 5)
        {
            var knockTime = TimeSpan.FromSeconds(
                Math.Min(comp.PunchMaxKnockdown, _random.NextFloat(1f, 1f * powerLevel)));
            _stun.TryKnockdown(target, knockTime, true);

            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-brawl-punch-knockdown-others",
                    ("user", ent.Owner), ("target", target)),
                Loc.GetString("bloodsucker-brawl-punch-knockdown-user", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Large);
        }
        else
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-brawl-punch-user", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
        }

        _audio.PlayPvs(comp.PunchSound, target);

        // EMP borgs in addition to the punch
        if (HasComp<SiliconComponent>(target))
        {
            _emp.EmpPulse(
                _transform.GetMapCoordinates(target),
                comp.EMPRadius,
                comp.EMPConsumption,
                comp.EMPDuration);
        }
    }

    private void StartBashDoAfter(Entity<BloodsuckerComponent> ent, EntityUid target,
        BloodsuckerBrawlComponent comp, bool isDoor)
    {
        var args = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            comp.BashDelay,
            new BloodsuckerBrawlDoAfterEvent { UserData = isDoor },
            ent.Owner,
            target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(args);
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerBrawlComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity),
                -comp.HumanityCost);

        return true;
    }

    #endregion
}
