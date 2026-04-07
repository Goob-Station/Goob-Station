using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Actions;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Melee;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Humanoid;
using Content.Goobstation.Maths.FixedPoint;
using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// System for the massacre action. Can be re-used for other similar weapons(Like the gang weapon if that ever gets made).
/// </summary>
public sealed class SlasherMassacreSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlasherMassacreMacheteComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<SlasherMassacreUserComponent, SlasherMassacreEvent>(OnMassacreAction);
        SubscribeLocalEvent<SlasherMassacreMacheteComponent, MeleeHitEvent>(OnMeleeHitWeapon);
        SubscribeLocalEvent<SlasherMassacreVictimComponent, MobStateChangedEvent>(OnVictimMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlasherMassacreUserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Active || comp.LastAttackTime == null)
                continue;

            var timeSinceLastAttack = _timing.CurTime - comp.LastAttackTime.Value;
            if (timeSinceLastAttack.TotalSeconds >= comp.ChainTimeoutSeconds)
                EndChain(uid, comp, true);
        }
    }

    private void OnGetItemActions(EntityUid uid, SlasherMassacreMacheteComponent comp, GetItemActionsEvent args)
    {
        EnsureComp<SlasherMassacreUserComponent>(args.User);

        if (_net.IsServer)
            args.AddAction(ref comp.MassacreActionEntity, comp.MassacreActionId);

        Dirty(uid, comp);
    }

    private void OnMassacreAction(Entity<SlasherMassacreUserComponent> ent, ref SlasherMassacreEvent args)
    {
        if (!ent.Comp.Active)
        {
            ent.Comp.Active = true;
            ent.Comp.HitCount = 0;
            ent.Comp.CurrentVictim = null;
            ent.Comp.LastAttackTime = _timing.CurTime;

            _popup.PopupPredicted(Loc.GetString("slasher-massacre-start"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            _audio.PlayPredicted(ent.Comp.MassacreIntro, ent.Owner, ent.Owner);

            _actions.StartUseDelay((EntityUid?) args.Action);

            if (TryGetMachete(ent.Owner, out var weaponUid, out var melee))
            {
                melee.CanWideSwing = false;
                Dirty(weaponUid, melee);
            }
        }
        else
            _popup.PopupEntity(Loc.GetString("slasher-massacre-already-activated"), ent.Owner, ent.Owner, PopupType.Medium);
        Dirty(ent);
    }

    private void EndChain(EntityUid uid, SlasherMassacreUserComponent comp, bool showPopup = false)
    {
        if (comp.Active && showPopup)
            _popup.PopupPredicted(Loc.GetString("slasher-massacre-end"), uid, uid, PopupType.MediumCaution);

        if (comp.CurrentVictim != null
            && TryComp<SlasherMassacreVictimComponent>(comp.CurrentVictim.Value, out _))
            RemCompDeferred<SlasherMassacreVictimComponent>(comp.CurrentVictim.Value);

        if (TryGetMachete(uid, out var weaponUid, out var weapon))
        {
            weapon.CanWideSwing = true;
            Dirty(weaponUid, weapon);
        }

        comp.Active = false;
        comp.HitCount = 0;
        comp.CurrentVictim = null;
        comp.LastAttackTime = null;
        Dirty(uid, comp);
    }

    private void OnVictimMobStateChanged(EntityUid uid, SlasherMassacreVictimComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (comp.Attacker != null
            && comp.WeaponComp != null
            && TryComp<SlasherMassacreUserComponent>(comp.Attacker.Value, out var userComp)
            && userComp.Active
            && userComp.CurrentVictim == uid
            && userComp.HitCount >= 1)
        {
            _audio.PlayPredicted(comp.MassacreFinale, comp.Attacker.Value, comp.Attacker.Value);
            _popup.PopupPredicted(Loc.GetString("slasher-massacre-decap"), comp.Attacker.Value, comp.Attacker.Value, PopupType.MediumCaution);

            ApplyKillBonuses(comp.Attacker.Value, userComp.HitCount, comp.WeaponComp);

            EndChain(comp.Attacker.Value, userComp);
        }

        RemCompDeferred<SlasherMassacreVictimComponent>(uid);
    }

    private void OnMeleeHitWeapon(Entity<SlasherMassacreMacheteComponent> weaponEnt, ref MeleeHitEvent args)
    {
        if (!TryComp<SlasherMassacreUserComponent>(args.User, out var userComp)
            || !userComp.Active) // don't activate when comp isn't active.
            return;

        // End the chain when you miss.
        if (args.HitEntities.Count == 0)
        {
            EndChain(args.User, userComp, true);
            return;
        }

        // Only consider humanoid's as targets.
        EntityUid? victim = null;
        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<HumanoidAppearanceComponent>(hit))
                continue;

            victim = hit;
            break;
        }

        // If no valid humanoid was hit, treat like a miss and end the chain.
        if (victim == null)
        {
            EndChain(args.User, userComp, true);
            return;
        }

        // End the chain if the victim is already dead.
        if (_mobState.IsDead(victim.Value))
        {
            _popup.PopupPredicted(Loc.GetString("slasher-massacre-end"), args.User, args.User, PopupType.MediumCaution);

            EndChain(args.User, userComp, true);
            return;
        }

        // When the target changes reset hitcount.
        if (userComp.CurrentVictim != null
            && userComp.CurrentVictim != victim.Value)
        {
            _popup.PopupPredicted(Loc.GetString("slasher-massacre-target-change"), args.User, args.User, PopupType.MediumCaution);

            // Remove victim component from old target
            if (TryComp<SlasherMassacreVictimComponent>(userComp.CurrentVictim.Value, out _))
                RemCompDeferred<SlasherMassacreVictimComponent>(userComp.CurrentVictim.Value);

            userComp.HitCount = 0;
        }

        userComp.CurrentVictim = victim.Value;
        userComp.HitCount++;
        userComp.LastAttackTime = _timing.CurTime;

        // Add the victim tracking component to monitor mob state changes
        var victimComp = EnsureComp<SlasherMassacreVictimComponent>(victim.Value);
        victimComp.Attacker = args.User;
        victimComp.WeaponComp = weaponEnt.Comp;
        Dirty(victim.Value, victimComp);

        // Calculate damage bonus/penalty.
        var totalBonus = -weaponEnt.Comp.BaseDamagePenalty + weaponEnt.Comp.PerHitBonus * (userComp.HitCount - 1);
        if (totalBonus != 0)
        {
            var spec = new DamageSpecifier();
            spec.DamageDict.Add("Slash", totalBonus);
            args.BonusDamage += spec;
        }

        var playedDelimb = false;

        // Limb severing phase. sever every 4 hits.
        if (userComp.HitCount >= weaponEnt.Comp.LimbSeverHits
            && userComp.HitCount % weaponEnt.Comp.LimbSeverHits == 0)
            if (TrySeverRandomLimb(victim.Value))
                playedDelimb = true;

        // Decapitation.
        if (userComp.HitCount == weaponEnt.Comp.DecapitateHit)
            if (Decapitate(victim.Value))
                playedDelimb = true;

        // Audio handling
        if (_net.IsServer)
        {
            if (playedDelimb)
                _audio.PlayPvs(weaponEnt.Comp.MassacreDelimb, args.User);
            else
                _audio.PlayPvs(weaponEnt.Comp.MassacreSlash, args.User);
        }

        Dirty(args.User, userComp);
    }

    /// <summary>
    /// Applies speed and health bonuses when a victim dies during massacre mode.
    /// Bonus scales with hit count.
    /// </summary>
    private void ApplyKillBonuses(EntityUid user, int hitCount, SlasherMassacreMacheteComponent comp)
    {
        if (!_net.IsServer || hitCount <= 0)
            return;

        var speedBonus = comp.SpeedBonusPerHit * hitCount;
        var healAmount = comp.HealAmountPerHit * hitCount;

        // Apply speed boost.
        if (speedBonus > 0)
        {
            var speedComp = EnsureComp<MovespeedModifierMetabolismComponent>(user);
            var speedMultiplier = 1f + speedBonus;
            var endTime = _timing.CurTime + TimeSpan.FromSeconds(comp.SpeedBoostDuration);

            // Only update if the modifier changed or we're extending the duration
            if (speedComp.ModifierTimer < endTime)
            {
                speedComp.WalkSpeedModifier = speedMultiplier;
                speedComp.SprintSpeedModifier = speedMultiplier;
                speedComp.ModifierTimer = endTime;

                Dirty(user, speedComp);
                _movementSpeed.RefreshMovementSpeedModifiers(user);
            }
        }

        // Apply healing via slasherium
        if (healAmount > 0 && TryComp<BloodstreamComponent>(user, out var bloodstream))
            if (_solutions.ResolveSolution(user, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution))
                _solutions.TryAddReagent(bloodstream.ChemicalSolution.Value,
                    new ReagentId(comp.HealReagent, null),
                    FixedPoint2.New(healAmount),
                    out _);
    }

    // Handles severing a random limb.
    private bool TrySeverRandomLimb(EntityUid victim)
    {
        var parts = _body.GetBodyChildren(victim);
        var severable = new List<EntityUid>();

        foreach (var part in parts)
            if (part.Component.PartType is BodyPartType.Arm or BodyPartType.Leg)
                severable.Add(part.Id);

        if (severable.Count == 0)
            return false;

        var pickedLimb = _random.Pick(severable);

        if (!TryComp<WoundableComponent>(pickedLimb, out var limbWoundable)
            || !limbWoundable.ParentWoundable.HasValue)
            return false;

        _wounds.AmputateWoundableSafely(limbWoundable.ParentWoundable.Value, pickedLimb, limbWoundable);

        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("slasher-massacre-limb"), victim, PopupType.Medium);
        return true;
    }

    // Handles decapitation.
    private bool Decapitate(EntityUid victim)
    {
        var parts = _body.GetBodyChildren(victim);
        EntityUid? head = null;
        EntityUid? chest = null;
        foreach (var part in parts)
        {
            switch (part.Component.PartType)
            {
                case BodyPartType.Head:
                    head = part.Id;
                    break;
                case BodyPartType.Chest:
                    chest = part.Id;
                    break;
            }
        }
        if (head == null || chest == null)
            return false;
        _wounds.AmputateWoundable(chest.Value, head.Value);
        return true;
    }

    /// <summary>
    /// Tries to get the machete held by the user.
    /// </summary>
    private bool TryGetMachete(EntityUid user, out EntityUid weaponUid, [NotNullWhen(true)] out MeleeWeaponComponent? melee)
    {
        weaponUid = default;
        melee = null;

        if (!TryComp<HandsComponent>(user, out var hands))
            return false;

        foreach (var held in _hands.EnumerateHeld((user, hands)))
            if (HasComp<SlasherMassacreMacheteComponent>(held) && TryComp<MeleeWeaponComponent>(held, out var weapon))
            {
                weaponUid = held;
                melee = weapon;
                return true;
            }

        return false;
    }
}
