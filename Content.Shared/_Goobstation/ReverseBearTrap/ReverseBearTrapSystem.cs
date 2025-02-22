using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Tools.Systems;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using System.Linq;
using Content.Shared.Tag;
using Content.Shared.IdentityManagement;
using Robust.Shared.Player;

namespace Content.Shared.ReverseBearTrap;

public sealed partial class ReverseBearTrapSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReverseBearTrapComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ReverseBearTrapComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<ReverseBearTrapComponent, GetVerbsEvent<Verb>>(OnVerbAdd);

        // DoAfter event handlers
        SubscribeLocalEvent<ReverseBearTrapComponent, BearTrapEscapeDoAfterEvent>(OnBearTrapEscape);
        SubscribeLocalEvent<ReverseBearTrapComponent, BearTrapApplyDoAfterEvent>(OnBearTrapApply);
        SubscribeLocalEvent<ReverseBearTrapComponent, WeldFinishedEvent>(OnWeldFinished);
        SubscribeLocalEvent<ReverseBearTrapComponent, BearTrapUnlockDoAfterEvent>(OnBearTrapUnlock);
    }

    private void OnEquipped(EntityUid uid, ReverseBearTrapComponent trap, GotEquippedEvent args)
    {
        if (args.Slot != "head")
            return;

        ArmTrap(uid, trap, args.Equipee);
    }

    private void OnMeleeHit(EntityUid uid, ReverseBearTrapComponent trap, MeleeHitEvent args)
    {
        if (args.Handled)
            return;

        // Ensure we're actually hitting a valid target
        if (args.HitEntities.Count == 0 ||
            !HasComp<HumanoidAppearanceComponent>(args.HitEntities.First()) ||
            _inventory.TryGetSlotEntity(args.HitEntities.First(), "head", out _))
            return;

        args.Handled = true;
        var target = args.HitEntities[0];
        var user = args.User;

        _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-start-cuffing-observer",
                    ("user", Identity.Name(user, EntityManager)), ("target", Identity.Name(target, EntityManager))),
                target, Filter.Pvs(target, entityManager: EntityManager)
                    .RemoveWhere(e => e.AttachedEntity == target || e.AttachedEntity == user), true);

        if (target == user)
        {
            _popup.PopupClient(Loc.GetString("reverse-bear-trap-component-target-self"), user, user);
        }
        else
        {
            _popup.PopupClient(Loc.GetString("reverse-bear-trap-component-start-cuffing-target",
                ("targetName", Identity.Name(target, EntityManager, user))), user, user);
            _popup.PopupClient(Loc.GetString("reverse-bear-trap-component-start-cuffing-by-other",
                ("otherName", Identity.Name(user, EntityManager, target))), target, target, PopupType.Large);
        }

        _audio.PlayPredicted(trap.StartCuffSound, uid, user);

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, 3f,
            new BearTrapApplyDoAfterEvent(), uid, target, uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnVerbAdd(EntityUid uid, ReverseBearTrapComponent trap, GetVerbsEvent<Verb> args)
    {
        if (!_actionBlockerSystem.CanComplexInteract(args.User))
            return;

        if (trap.Ticking && trap.Wearer.HasValue)
        {
            var activeItem = _handsSystem.GetActiveItem(args.User);
            if (args.User == trap.Wearer)
            {
                args.Verbs.Add(new Verb()
                {
                    Act = () => AttemptEscape(uid, trap, args.User),
                    DoContactInteraction = true,
                    Text = "Attempt escape"
                });
            }
            else
            {
                args.Verbs.Add(new Verb()
                {
                    DoContactInteraction = true,
                    Text = "Remove trap",
                    Disabled = !activeItem.HasValue || !_toolSystem.HasQuality(activeItem.Value, "Welding"),
                    Act = () =>
                    {
                        var user = args.User;
                        var target = trap.Wearer.Value;
                        _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-start-welding-observer",
                            ("user", Identity.Name(user, EntityManager)), ("target", Identity.Name(target, EntityManager))),
                            target, Filter.Pvs(target, entityManager: EntityManager)
                            .RemoveWhere(e => e.AttachedEntity == target || e.AttachedEntity == user), true);

                        _popup.PopupClient(Loc.GetString("reverse-bear-trap-component-start-welding-target",
                            ("targetName", Identity.Name(target, EntityManager, user))), user, user);
                        _popup.PopupClient(Loc.GetString("reverse-bear-trap-component-start-welding-by-other",
                            ("otherName", Identity.Name(user, EntityManager, target))), target, target, PopupType.Large);

                        _toolSystem.UseTool(activeItem!.Value, args.User, uid, 5f, "Welding", new WeldFinishedEvent(), 3f);
                    }
                });
            }

            if (activeItem.HasValue
                && TryComp<TagComponent>(activeItem, out var tagComponent)
                && _tag.HasTag(tagComponent, "ReverseBearTrapKey"))
            {
                args.Verbs.Add(new Verb()
                {
                    DoContactInteraction = true,
                    Text = "Unlock trap",
                    Act = () =>
                    {
                        var user = args.User;
                        var target = trap.Wearer.Value;
                        _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-start-unlocking-observer",
                            ("user", Identity.Name(user, EntityManager)), ("target", Identity.Name(target, EntityManager))),
                            target, Filter.Pvs(target, entityManager: EntityManager)
                            .RemoveWhere(e => e.AttachedEntity == target || e.AttachedEntity == user), true);

                        if (target == user)
                        {
                            _popup.PopupClient(Loc.GetString("reverse-bear-trap-component-start-unlocking-target-self"), user, user);
                        }
                        else
                        {
                            _popup.PopupClient(Loc.GetString("reverse-bear-trap-component-start-unlocking-target",
                                ("targetName", Identity.Name(target, EntityManager, user))), user, user);
                            _popup.PopupClient(Loc.GetString("reverse-bear-trap-component-start-unlocking-by-other",
                                ("otherName", Identity.Name(user, EntityManager, target))), target, target, PopupType.Large);
                        }

                        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, 1.5f,
                            new BearTrapUnlockDoAfterEvent(), uid, uid)
                        {
                            BreakOnDamage = true,
                            BreakOnMove = true,
                            AttemptFrequency = AttemptFrequency.EveryTick
                        };

                        _doAfter.TryStartDoAfter(doAfterArgs);
                    }
                });
            }
        }
        else
        {
            if (trap.DelayOptions == null || trap.DelayOptions.Count == 1)
                return;

            foreach (var option in trap.DelayOptions)
            {
                if (MathHelper.CloseTo(option, trap.CountdownDuration))
                {
                    args.Verbs.Add(new Verb()
                    {
                        Category = TimerOptions,
                        Text = Loc.GetString("verb-trigger-timer-set-current", ("time", option)),
                        Disabled = true,
                        Priority = (int) (-100 * option)
                    });
                    continue;
                }

                args.Verbs.Add(new Verb()
                {
                    Category = TimerOptions,
                    Text = Loc.GetString("verb-trigger-timer-set", ("time", option)),
                    Priority = (int) (-100 * option),

                    Act = () =>
                    {
                        trap.CountdownDuration = option;
                        if (_net.IsServer)
                            _popup.PopupEntity(Loc.GetString("popup-trigger-timer-set", ("time", option)), args.User, args.User);
                    },
                });
            }
        }
    }

    private void OnBearTrapEscape(EntityUid uid, ReverseBearTrapComponent trap, BearTrapEscapeDoAfterEvent args)
    {
        if (_net.IsClient || args.Cancelled || trap.Wearer == null)
            return;

        trap.Struggling = false;

        if (_random.NextFloat() * 100 < trap.CurrentEscapeChance)
        {
            _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-unlocked-trap-observer",
                    ("user", Identity.Name(trap.Wearer.Value, EntityManager))),
                uid, Filter.Pvs(uid, entityManager: EntityManager)
                    .RemoveWhere(e => e.AttachedEntity == uid || e.AttachedEntity == trap.Wearer), true);

            _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-unlocked-trap-self"), trap.Wearer.Value, trap.Wearer.Value);

            ResetTrap(uid, trap);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-failed-unlocked-trap-observer",
                    ("user", Identity.Name(trap.Wearer.Value, EntityManager))),
                uid, Filter.Pvs(uid, entityManager: EntityManager)
                    .RemoveWhere(e => e.AttachedEntity == uid || e.AttachedEntity == trap.Wearer), true);

            _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-failed-unlocked-trap-self"), trap.Wearer.Value, trap.Wearer.Value);

            trap.CurrentEscapeChance += 0.25f;
        }
    }

    private void OnBearTrapApply(EntityUid uid, ReverseBearTrapComponent trap, BearTrapApplyDoAfterEvent args)
    {
        if (args.Cancelled || args.Target is not { } target || args.Used is not { } used)
            return;

        if (!_inventory.TryGetSlotEntity(target, "head", out var _)
            && _inventory.TryEquip(target, used, "head", true, true))
            ArmTrap(used, trap, target);
    }

    private void OnWeldFinished(EntityUid uid, ReverseBearTrapComponent trap, WeldFinishedEvent args)
    {
        if (_net.IsClient || args.Cancelled || args.Used == null || !trap.Wearer.HasValue)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Heat", 50);
        _damageable.TryChangeDamage(trap.Wearer, damage, true, origin: args.Used, targetPart: _Shitmed.Targeting.TargetBodyPart.Head);

        _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-trap-fall-observer",
                    ("user", Identity.Name(trap.Wearer.Value, EntityManager))),
                uid, Filter.Pvs(uid, entityManager: EntityManager)
                    .RemoveWhere(e => e.AttachedEntity == uid || e.AttachedEntity == trap.Wearer), true);

        _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-trap-fall-self"), trap.Wearer.Value, trap.Wearer.Value);

        ResetTrap(uid, trap);
    }

    private void OnBearTrapUnlock(EntityUid uid, ReverseBearTrapComponent trap, BearTrapUnlockDoAfterEvent args)
    {
        if (_net.IsClient || args.Cancelled || !trap.Wearer.HasValue)
            return;

        _audio.PlayPredicted(trap.StartCuffSound, trap.Wearer.Value, null);

        _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-trap-fall-observer",
                    ("user", Identity.Name(trap.Wearer.Value, EntityManager))),
                uid, Filter.Pvs(uid, entityManager: EntityManager)
                    .RemoveWhere(e => e.AttachedEntity == uid || e.AttachedEntity == trap.Wearer), true);

        _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-trap-fall-self"), trap.Wearer.Value, trap.Wearer.Value);

        ResetTrap(uid, trap);
    }

    private void ArmTrap(EntityUid uid, ReverseBearTrapComponent trap, EntityUid wearer)
    {
        if (trap.Ticking || !EntityManager.EntityExists(wearer) || !_interaction.InRangeUnobstructed(uid, wearer))
            return;

        trap.Ticking = true;
        trap.ActivateTime = _gameTiming.CurTime;
        trap.Wearer = wearer;
        trap.CurrentEscapeChance = trap.BaseEscapeChance;
        EnsureComp<UnremoveableComponent>(uid);

        Dirty(uid, trap);

        if (_net.IsServer)
        {
            _audio.PlayPredicted(trap.BeepSound, uid, null,
                AudioParams.Default.WithVolume(-5f));

            trap.LoopSoundStream = _audio.PlayPredicted(trap.LoopSound, uid, null,
            AudioParams.Default.WithLoop(true))?.Entity;

            _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-trap-click-observer",
                ("user", Identity.Name(trap.Wearer.Value, EntityManager))),
                uid, Filter.Pvs(uid, entityManager: EntityManager)
                .RemoveWhere(e => e.AttachedEntity == uid || e.AttachedEntity == trap.Wearer), true);

            _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-trap-click-self"), trap.Wearer.Value, trap.Wearer.Value);
        }
    }

    private void ResetTrap(EntityUid? uid, ReverseBearTrapComponent trap)
    {
        if (!trap.Ticking || !uid.HasValue)
            return;

        var oldWearer = trap.Wearer;

        trap.LoopSoundStream = _audio.Stop(trap.LoopSoundStream);
        trap.Ticking = false;
        trap.Wearer = null;
        trap.Struggling = false;
        trap.CurrentEscapeChance = trap.BaseEscapeChance;
        RemComp<UnremoveableComponent>(uid.Value);

        Dirty(uid.Value, trap);

        if (oldWearer != null && TryComp<ItemComponent>(uid, out var _))
            _inventory.TryUnequip(oldWearer.Value, "head", true, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ReverseBearTrapComponent>();
        while (query.MoveNext(out var uid, out var trap))
        {
            if (!trap.Ticking || trap.Wearer == null)
                continue;

            var remaining = trap.CountdownDuration - (float) (_gameTiming.CurTime - trap.ActivateTime).TotalSeconds;
            if (remaining <= 0)
            {
                SnapTrap(uid, trap);
                continue;
            }
        }
    }

    private void SnapTrap(EntityUid uid, ReverseBearTrapComponent? trap)
    {
        if (!Resolve(uid, ref trap) || trap.Wearer == null)
            return;

        if (_net.IsServer)
        {
            _audio.PlayPredicted(trap.SnapSound, trap.Wearer.Value, null);

            _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-trap-snap-observer",
                ("user", Identity.Name(trap.Wearer.Value, EntityManager))),
                uid, Filter.Pvs(uid, entityManager: EntityManager)
                .RemoveWhere(e => e.AttachedEntity == uid || e.AttachedEntity == trap.Wearer), true, PopupType.LargeCaution);

            _popup.PopupEntity(Loc.GetString("reverse-bear-trap-component-trap-snap-self"), trap.Wearer.Value, trap.Wearer.Value, PopupType.LargeCaution);
        }

        var wearer = trap.Wearer;

        // damage destroys trap
        ResetTrap(uid, trap);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 300);
        _damageable.TryChangeDamage(wearer, damage, true, origin: uid, targetPart: _Shitmed.Targeting.TargetBodyPart.Head);
    }

    private void AttemptEscape(EntityUid uid, ReverseBearTrapComponent trap, EntityUid user)
    {
        if (trap.Struggling)
            return;

        trap.Struggling = true;

        var doAfterArgs = new DoAfterArgs(EntityManager, user, 6f,
            new BearTrapEscapeDoAfterEvent(), uid, user)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    [Serializable, NetSerializable]
    private sealed partial class BearTrapEscapeDoAfterEvent : SimpleDoAfterEvent { }
    [Serializable, NetSerializable]
    private sealed partial class BearTrapApplyDoAfterEvent : SimpleDoAfterEvent { }
    [Serializable, NetSerializable]
    private sealed partial class BearTrapUnlockDoAfterEvent : SimpleDoAfterEvent { }

    private static readonly VerbCategory TimerOptions = new("verb-categories-timer", "/Textures/Interface/VerbIcons/clock.svg.192dpi.png");
}
