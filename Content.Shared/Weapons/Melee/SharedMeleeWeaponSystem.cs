// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Common.Weapons; // Goobstation - Martial Arts
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions.Events;
using Content.Shared.Administration.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Components; // DeltaV - Melee Stamina Resistance Override
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Lavaland.Weapons;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Coordinates;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using ItemToggleMeleeWeaponComponent = Content.Shared.Item.ItemToggle.Components.ItemToggleMeleeWeaponComponent;

namespace Content.Shared.Weapons.Melee;

public abstract class SharedMeleeWeaponSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly IMapManager MapManager = default!;
    [Dependency] private   readonly INetManager _netMan = default!;
    [Dependency] private   readonly IPrototypeManager _protoManager = default!;
    [Dependency] private   readonly IRobustRandom _random = default!;
    [Dependency] protected readonly ISharedAdminLogManager AdminLogger = default!;
    [Dependency] protected readonly ActionBlockerSystem Blocker = default!;
    [Dependency] protected readonly DamageableSystem Damageable = default!;
    [Dependency] private   readonly SharedHandsSystem _hands = default!;
    [Dependency] private   readonly InventorySystem _inventory = default!;
    [Dependency] private   readonly MeleeSoundSystem _meleeSound = default!;
    [Dependency] protected readonly MobStateSystem MobState = default!;
    [Dependency] private   readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly SharedCombatModeSystem CombatMode = default!;
    [Dependency] protected readonly SharedInteractionSystem Interaction = default!;
    [Dependency] private   readonly SharedPhysicsSystem _physics = default!;
    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] private   readonly ContestsSystem _contests = default!;
    [Dependency] private   readonly ThrowingSystem _throwing = default!;
    [Dependency] private   readonly INetConfigurationManager _config = default!;
    [Dependency] private   readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private   readonly DamageExamineSystem _damageExamine = default!;
    [Dependency] private   readonly TagSystem _tag = default!;

    //Goob - Shove
    private float _shoveRange;
    private float _shoveSpeed;
    private float _shoveMass;
    //Goob - Shove

    // Goobstation - Shove
    private void SetShoveRange(float value)
    {
        _shoveRange = value;
    }

    private void SetShoveSpeed(float value)
    {
        _shoveSpeed = value;
    }

    private void SetShoveMass(float value)
    {
        _shoveMass = value;
    }
    //Goob - Shove

    public const int AttackMask = (int) (CollisionGroup.MobMask | CollisionGroup.Opaque); // WD EDIT: private -> public

    /// <summary>
    /// Maximum amount of targets allowed for a wide-attack.
    /// </summary>
    public const int MaxTargets = 5;

    /// <summary>
    /// If an attack is released within this buffer it's assumed to be full damage.
    /// </summary>
    public const float GracePeriod = 0.05f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeleeWeaponComponent, HandSelectedEvent>(OnMeleeSelected);
        SubscribeLocalEvent<MeleeWeaponComponent, ShotAttemptedEvent>(OnMeleeShotAttempted);
        SubscribeLocalEvent<MeleeWeaponComponent, GunShotEvent>(OnMeleeShot);
        SubscribeLocalEvent<MeleeWeaponComponent, DamageExamineEvent>(OnMeleeExamineDamage);
        SubscribeLocalEvent<BonusMeleeDamageComponent, GetMeleeDamageEvent>(OnGetBonusMeleeDamage);
        SubscribeLocalEvent<BonusMeleeDamageComponent, GetHeavyDamageModifierEvent>(OnGetBonusHeavyDamageModifier);
        SubscribeLocalEvent<BonusMeleeAttackRateComponent, GetMeleeAttackRateEvent>(OnGetBonusMeleeAttackRate);

        SubscribeLocalEvent<ItemToggleMeleeWeaponComponent, ItemToggledEvent>(OnItemToggle);

        SubscribeAllEvent<HeavyAttackEvent>(OnHeavyAttack);
        SubscribeAllEvent<LightAttackEvent>(OnLightAttack);
        SubscribeAllEvent<DisarmAttackEvent>(OnDisarmAttack);
        SubscribeAllEvent<StopAttackEvent>(OnStopAttack);

        Subs.CVar(_config, GoobCVars.ShoveRange, SetShoveRange, true);
        Subs.CVar(_config, GoobCVars.ShoveSpeed, SetShoveSpeed, true);
        Subs.CVar(_config, GoobCVars.ShoveMassFactor, SetShoveMass, true);

#if DEBUG
        SubscribeLocalEvent<MeleeWeaponComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, MeleeWeaponComponent component, MapInitEvent args)
    {
        if (component.NextAttack > Timing.CurTime)
            Log.Warning($"Initializing a map that contains an entity that is on cooldown. Entity: {ToPrettyString(uid)}");
#endif
    }

    private void OnMeleeShotAttempted(EntityUid uid, MeleeWeaponComponent comp, ref ShotAttemptedEvent args)
    {
        if (comp.NextAttack > Timing.CurTime)
            args.Cancel();
    }

    private void OnMeleeShot(EntityUid uid, MeleeWeaponComponent component, ref GunShotEvent args)
    {
        if (!TryComp<GunComponent>(uid, out var gun))
            return;

        if (gun.NextFire > component.NextAttack)
        {
            component.NextAttack = gun.NextFire;
            DirtyField(uid, component, nameof(MeleeWeaponComponent.NextAttack));
        }
    }

    private void OnMeleeExamineDamage(EntityUid uid, MeleeWeaponComponent component, ref DamageExamineEvent args)
    {
        if (component.Hidden)
            return;

        var damageSpec = GetDamage(uid, args.User, component);

        if (damageSpec.Empty)
            return;

        _damageExamine.AddDamageExamine(args.Message, Damageable.ApplyUniversalAllModifiers(damageSpec), Loc.GetString("damage-melee"));
    }
    private void OnMeleeSelected(EntityUid uid, MeleeWeaponComponent component, HandSelectedEvent args)
    {
        var attackRate = GetAttackRate(uid, args.User, component);
        if (attackRate.Equals(0f))
            return;

        if (!component.ResetOnHandSelected)
            return;

        if (Paused(uid))
            return;

        // If someone swaps to this weapon then reset its cd.
        var curTime = Timing.CurTime;
        var minimum = curTime + TimeSpan.FromSeconds(1 / attackRate);

        if (minimum < component.NextAttack)
            return;

        component.NextAttack = minimum;
        DirtyField(uid, component, nameof(MeleeWeaponComponent.NextAttack));
    }

    private void OnGetBonusMeleeDamage(EntityUid uid, BonusMeleeDamageComponent component, ref GetMeleeDamageEvent args)
    {
        if (component.BonusDamage != null)
            args.Damage += component.BonusDamage;
        if (component.DamageModifierSet != null)
            args.Modifiers.Add(component.DamageModifierSet);
    }

    private void OnGetBonusHeavyDamageModifier(EntityUid uid, BonusMeleeDamageComponent component, ref GetHeavyDamageModifierEvent args)
    {
        args.DamageModifier += component.HeavyDamageFlatModifier;
        args.Multipliers *= component.HeavyDamageMultiplier;
    }

    private void OnGetBonusMeleeAttackRate(EntityUid uid, BonusMeleeAttackRateComponent component, ref GetMeleeAttackRateEvent args)
    {
        args.Rate += component.FlatModifier;
        args.Multipliers *= component.Multiplier;
    }

    private void OnStopAttack(StopAttackEvent msg, EntitySessionEventArgs args)
    {
        var user = args.SenderSession.AttachedEntity;

        if (user == null)
            return;

        if (!TryGetWeapon(user.Value, out var weaponUid, out var weapon) ||
            weaponUid != GetEntity(msg.Weapon))
        {
            return;
        }

        if (!weapon.Attacking)
            return;

        weapon.Attacking = false;
        DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.Attacking));
    }

    private void OnLightAttack(LightAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user
            || TerminatingOrDeleted(user)) // Goob change
            return;

        if (!TryGetWeapon(user, out var weaponUid, out var weapon) ||
            weaponUid != GetEntity(msg.Weapon))
        {
            return;
        }

        AttemptAttack(user, weaponUid, weapon, msg, args.SenderSession);
    }

    private void OnHeavyAttack(HeavyAttackEvent msg, EntitySessionEventArgs args)
    {
        var weapon = GetEntity(msg.Weapon);
        if (args.SenderSession.AttachedEntity is not { } user
            || TerminatingOrDeleted(user)
            || TerminatingOrDeleted(weapon)) // Goobstation Change
            return;

        if (!TryGetWeapon(user, out var weaponUid, out var weaponComp)
            || weaponUid != weapon
            || !weaponComp.CanWideSwing) // Goobstation Change
            return;

        AttemptAttack(user, weaponUid, weaponComp, msg, args.SenderSession);
    }

    private void OnDisarmAttack(DisarmAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user
            || TerminatingOrDeleted(user)) // Goobstation Change
            return;

        if (TryGetWeapon(user, out var weaponUid, out var weapon))
            AttemptAttack(user, weaponUid, weapon, msg, args.SenderSession);
    }

    /// <summary>
    /// Gets the total damage a weapon does, including modifiers like wielding and enablind/disabling
    /// </summary>
    public DamageSpecifier GetDamage(EntityUid uid, EntityUid user, MeleeWeaponComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return new DamageSpecifier();

        var ev = new GetMeleeDamageEvent(uid, new(component.Damage * Damageable.UniversalMeleeDamageModifier), new(), user, component.ResistanceBypass);
        RaiseLocalEvent(uid, ref ev);
        // <Goobstation> - raise an event on the user too for strength augments
        var userEv = new GetUserMeleeDamageEvent(uid, ev.Damage, ev.Modifiers);
        RaiseLocalEvent(user, ref userEv);
        // this currently does nothing since they are classes, but it's futureproofing for struct DamageSpecifier.
        ev.Damage = userEv.Damage;
        ev.Modifiers = userEv.Modifiers;
        // </Goobstation>

        // Begin DeltaV additions
        // Allow users of melee weapons to have bonuses applied
        if (user != uid)
        {
            RaiseLocalEvent(user, ref ev);
        }

        return DamageSpecifier.ApplyModifierSets(ev.Damage, ev.Modifiers);
        // End DeltaV additions
    }

    public float GetAttackRate(EntityUid uid, EntityUid user, MeleeWeaponComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return 0;

        var ev = new GetMeleeAttackRateEvent(uid, component.AttackRate, 1, user);
        RaiseLocalEvent(uid, ref ev);
        if (user != uid) // Goobstation
            RaiseLocalEvent(user, ref ev);

        return ev.Rate * ev.Multipliers;
    }

    public FixedPoint2 GetHeavyDamageModifier(EntityUid uid, EntityUid user, MeleeWeaponComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return FixedPoint2.Zero;

        var ev = new GetHeavyDamageModifierEvent(uid, component.ClickDamageModifier, 1, user);
        RaiseLocalEvent(uid, ref ev);

        return ev.DamageModifier * ev.Multipliers;
    }

    public bool GetResistanceBypass(EntityUid uid, EntityUid user, MeleeWeaponComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        var ev = new GetMeleeDamageEvent(uid, new(component.Damage * Damageable.UniversalMeleeDamageModifier), new(), user, component.ResistanceBypass);
        RaiseLocalEvent(uid, ref ev);

        return ev.ResistanceBypass;
    }

    public bool TryGetWeapon(EntityUid entity, out EntityUid weaponUid, [NotNullWhen(true)] out MeleeWeaponComponent? melee)
    {
        weaponUid = default;
        melee = null;

        var ev = new GetMeleeWeaponEvent();
        RaiseLocalEvent(entity, ev);
        if (ev.Handled)
        {
            if (TryComp(ev.Weapon, out melee))
            {
                weaponUid = ev.Weapon.Value;
                return true;
            }

            return false;
        }

        // Use inhands entity if we got one.
        if (_hands.TryGetActiveItem(entity, out var held))
        {
            // Make sure the entity is a weapon AND it doesn't need
            // to be equipped to be used (E.g boxing gloves).
            if (TryComp(held, out melee) &&
                !melee.MustBeEquippedToUse)
            {
                // Lavaland Change start
                if (HasComp<MeleeWeaponRelayComponent>(held.Value))
                {
                    var relay = new GetRelayMeleeWeaponEvent();
                    RaiseLocalEvent(held.Value, ref relay);
                    if (relay.Handled && TryComp(relay.Found, out MeleeWeaponComponent? relayMelee))
                    {
                        weaponUid = relay.Found.Value;
                        melee = relayMelee;
                        return true;
                    }
                }
                // Lavaland Change end

                weaponUid = held.Value;
                return true;
            }

            if (!HasComp<VirtualItemComponent>(held))
                return false;
        }

        // Use hands clothing if applicable.
        if (_inventory.TryGetSlotEntity(entity, "gloves", out var gloves) &&
            TryComp<MeleeWeaponComponent>(gloves, out var glovesMelee))
        {
            weaponUid = gloves.Value;
            melee = glovesMelee;
            return true;
        }

        // Use our own melee
        if (TryComp(entity, out melee))
        {
            weaponUid = entity;
            return true;
        }

        return false;
    }

    public void AttemptLightAttackMiss(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, EntityCoordinates coordinates)
    {
        AttemptAttack(user, weaponUid, weapon, new LightAttackEvent(null, GetNetEntity(weaponUid), GetNetCoordinates(coordinates)), null);
    }

    public bool AttemptLightAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, EntityUid target)
    {
        if (!TryComp(target, out TransformComponent? targetXform))
            return false;

        return AttemptAttack(user, weaponUid, weapon, new LightAttackEvent(GetNetEntity(target), GetNetEntity(weaponUid), GetNetCoordinates(targetXform.Coordinates)), null);
    }

    // Goobstation
    public bool AttemptHeavyAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, List<EntityUid> targets, EntityCoordinates coordinates)
    {
        return AttemptAttack(user,
            weaponUid,
            weapon,
            new HeavyAttackEvent(GetNetEntity(weaponUid), GetNetEntityList(targets), GetNetCoordinates(coordinates)),
            null);
    }

    public bool AttemptDisarmAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, EntityUid target)
    {
        if (!TryComp(target, out TransformComponent? targetXform))
            return false;

        return AttemptAttack(user, weaponUid, weapon, new DisarmAttackEvent(GetNetEntity(target), GetNetCoordinates(targetXform.Coordinates)), null);
    }

    /// <summary>
    /// Called when a windup is finished and an attack is tried.
    /// </summary>
    /// <returns>True if attack successful</returns>
    private bool AttemptAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, AttackEvent attack, ICommonSession? session)
    {
        var curTime = Timing.CurTime;

        if (weapon.NextAttack > curTime)
            return false;

        if (!CombatMode.IsInCombatMode(user))
            return false;

        EntityUid? target = null;
        switch (attack)
        {
            case LightAttackEvent light:
                if (light.Target != null && !TryGetEntity(light.Target, out target))
                {
                    // Target was lightly attacked & deleted.
                    return false;
                }

                // <Trauma>
                if (TryComp(target, out TargetInteractionRelayComponent? relay) && relay.RelayMelee &&
                    Exists(relay.RelayEntity) && relay.RelayEntity.Value != target)
                {
                    return AttemptAttack(user,
                        weaponUid,
                        weapon,
                        new LightAttackEvent(GetNetEntity(relay.RelayEntity.Value), light.Weapon, light.Coordinates),
                        session);
                }
                // </Trauma>

                if (!Blocker.CanAttack(user, target, (weaponUid, weapon)))
                    return false;

                // Can't self-attack if you're the weapon
                if (weaponUid == target)
                    return false;

                // Goobstation start
                var specialEv = new LightAttackSpecialInteractionEvent(target, user, weapon.Range);
                RaiseLocalEvent(weaponUid, ref specialEv);
                if (specialEv.Cancel)
                    return false;
                // Goobstation end

                break;
            case DisarmAttackEvent disarm:
                if (disarm.Target != null && !TryGetEntity(disarm.Target, out target))
                {
                    // Target was lightly attacked & deleted.
                    return false;
                }

                // <Trauma>
                if (TryComp(target, out relay) && relay.RelayMelee && Exists(relay.RelayEntity))
                {
                    return AttemptAttack(user,
                        weaponUid,
                        weapon,
                        new DisarmAttackEvent(GetNetEntity(relay.RelayEntity.Value), disarm.Coordinates),
                        session);
                }
                // </Trauma>

                if (!Blocker.CanAttack(user, target, (weaponUid, weapon), true))
                    return false;
                break;
            default:
                if (!weapon.CanHeavyAttack) // Goobstation
                    return false;

                if (!Blocker.CanAttack(user, weapon: (weaponUid, weapon)))
                    return false;
                break;
        }

        // Windup time checked elsewhere.
        var fireRate = TimeSpan.FromSeconds(1f / GetAttackRate(weaponUid, user, weapon));
        var swings = 0;

        // TODO: If we get autoattacks then probably need a shotcounter like guns so we can do timing properly.
        if (weapon.NextAttack < curTime)
            weapon.NextAttack = curTime;

        while (weapon.NextAttack <= curTime)
        {
            weapon.NextAttack += fireRate;
            swings++;
        }

        DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.NextAttack));

        // Do this AFTER attack so it doesn't spam every tick
        var ev = new AttemptMeleeEvent(user, weaponUid, weapon, attack is HeavyAttackEvent); // Goob edit
        RaiseLocalEvent(weaponUid, ref ev);
        RaiseLocalEvent(user, ref ev); // Shitmed Change

        if (weapon.SwingBeverage)
        {
            weapon.SwingLeft = !weapon.SwingLeft;
            DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.SwingLeft));
        }

        if (ev.Cancelled)
        {
            if (ev.Message != null)
            {
                PopupSystem.PopupClient(ev.Message, weaponUid, user);
            }

            return false;
        }
        // Shitmed Change End

        // Attack confirmed
        for (var i = 0; i < swings; i++)
        {
            EntProtoId animation; // Goobstation - Edit
            var spriteRotation = weapon.AnimationRotation;

            switch (attack)
            {
                case LightAttackEvent light:
                    DoLightAttack(user, light, weaponUid, weapon, session);
                    break;
                case DisarmAttackEvent disarm:
                    if (!DoDisarm(user, disarm, weaponUid, weapon, session)) // Goob edit
                        return false;

                    animation = weapon.DisarmAnimation; // WWDP
                    DoLungeAnimation(user, weaponUid, weapon.Angle, TransformSystem.ToMapCoordinates(GetCoordinates(attack.Coordinates)), weapon.Range, animation, spriteRotation, weapon.FlipAnimation); // Goobstation - Edit
                    break;
                case HeavyAttackEvent heavy:
                    if (!DoHeavyAttack(user, heavy, weaponUid, weapon, session))
                        return false;

                    animation = weapon.WideAnimation;
                    spriteRotation = weapon.WideAnimationRotation;
                    DoLungeAnimation(user, weaponUid, weapon.Angle, TransformSystem.ToMapCoordinates(GetCoordinates(attack.Coordinates)), weapon.Range, animation, spriteRotation, weapon.FlipAnimation); // Goobstation - Edit
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        var attackEv = new MeleeAttackEvent(weaponUid);
        RaiseLocalEvent(user, ref attackEv);

        weapon.Attacking = true;
        DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.Attacking));
        return true;
    }

    public abstract bool InRange(EntityUid user, EntityUid target, float range, ICommonSession? session); // Goob edit

    // Goob edit
    public virtual void DoLightAttack(EntityUid user, LightAttackEvent ev, EntityUid meleeUid, MeleeWeaponComponent component, ICommonSession? session) // Goobstation - Edit
    {
        // If I do not come back later to fix Light Attacks being Heavy Attacks you can throw me in the spider pit -Errant
        var damage = GetDamage(meleeUid, user, component) * GetHeavyDamageModifier(meleeUid, user, component);
        var coords = GetCoordinates(ev.Coordinates); // Goobstation
        var weapon = GetEntity(ev.Weapon); // Goobstation - Edit
        var target = GetEntity(ev.Target);
        var resistanceBypass = GetResistanceBypass(meleeUid, user, component);

        // Goobstation start
        var rangeEv = new GetLightAttackRangeEvent(target, user, component.Range);
        RaiseLocalEvent(meleeUid, ref rangeEv);
        // Not in LOS.
        if (target != null && !InRange(user, target.Value, rangeEv.Cancel ? component.Range : rangeEv.Range, session))
            return;
        // Goobstation end

        // For consistency with wide attacks stuff needs damageable.
        if (Deleted(target) ||
            !HasComp<DamageableComponent>(target) ||
            !TryComp(target, out TransformComponent? targetXform)) // Goob edit
        {
            // Leave IsHit set to true, because the only time it's set to false
            // is when a melee weapon is examined. Misses are inferred from an
            // empty HitEntities.
            // TODO: This needs fixing
            if (meleeUid == user)
            {
                AdminLogger.Add(LogType.MeleeHit,
                    LogImpact.Low,
                    $"{ToPrettyString(user):actor} melee attacked (light) using their hands and missed");
            }
            else
            {
                AdminLogger.Add(LogType.MeleeHit,
                    LogImpact.Low,
                    $"{ToPrettyString(user):actor} melee attacked (light) using {ToPrettyString(meleeUid):tool} and missed");
            }
            var missEvent = new MeleeHitEvent(new List<EntityUid>(), user, meleeUid, damage, null, GetCoordinates(ev.Coordinates)); // Goob edit
            RaiseLocalEvent(meleeUid, missEvent, true); // Goob station - broadcast
            _meleeSound.PlaySwingSound(user, meleeUid, component);
            DoLungeAnimation(user, weapon, component.Angle, TransformSystem.ToMapCoordinates(ev.Coordinates), rangeEv.Range, component.MissAnimation, component.AnimationRotation, component.FlipAnimation); // Goobstation - Edit
            return;
        }

        // Goobstation start
        var beforeEvent = new BeforeHarmfulActionEvent(user, HarmfulActionType.Harm);
        RaiseLocalEvent(target.Value, beforeEvent);
        if (beforeEvent.Cancelled)
            return;
        // Goobstation end

        // Sawmill.Debug($"Melee damage is {damage.Total} out of {component.Damage.Total}");

        // Raise event before doing damage so we can cancel damage if the event is handled
        var hitEvent = new MeleeHitEvent(new List<EntityUid> { target.Value }, user, meleeUid, damage, null, GetCoordinates(ev.Coordinates)); // Goob edit
        RaiseLocalEvent(meleeUid, hitEvent, true); // Goob station - broadcast


        if (hitEvent.Handled)
            return;

        var targets = new List<EntityUid>(1)
        {
            target.Value
        };

        DoLungeAnimation(user, weapon, component.Angle, TransformSystem.ToMapCoordinates(target.Value.ToCoordinates()), rangeEv.Range, component.Animation, component.AnimationRotation, component.FlipAnimation); // Goobstation - Edit
        // We skip weapon -> target interaction, as forensics system applies DNA on hit
        Interaction.DoContactInteraction(user, weapon);

        // If the user is using a long-range weapon, this probably shouldn't be happening? But I'll interpret melee as a
        // somewhat messy scuffle. See also, heavy attacks.
        Interaction.DoContactInteraction(user, target);

        // For stuff that cares about it being attacked.
        var attackedEvent = new AttackedEvent(meleeUid, user, targetXform.Coordinates);
        RaiseLocalEvent(target.Value, attackedEvent);
        var modifiedDamage = DamageSpecifier.ApplyModifierSets(damage + hitEvent.BonusDamage + attackedEvent.BonusDamage, hitEvent.ModifiersList);
        modifiedDamage = DamageSpecifier.ApplyModifierSets(modifiedDamage, attackedEvent.ModifiersList); // Goobstation
        var damageResult = Damageable.TryChangeDamage(target, modifiedDamage, origin: user, partMultiplier: component.ClickPartDamageMultiplier); // Shitmed Change
        var comboEv = new ComboAttackPerformedEvent(user, target.Value, meleeUid, ComboAttackType.Harm);
        RaiseLocalEvent(user, comboEv);

        if (damageResult is {Empty: false})
        {
            // If the target has stamina and is taking blunt damage, they should also take stamina damage based on their blunt to stamina factor
            if (damageResult.DamageDict.TryGetValue("Blunt", out var bluntDamage))
            {
                //DeltaV - No Stamina Resistance Doubledipping, unless we want to.
                var ignoreResist = true;
                if (TryComp<StaminaResistanceComponent>(target, out var staminaResist))
                    ignoreResist = !staminaResist.MeleeResistance;

                _stamina.TakeStaminaDamage(target.Value, (bluntDamage * component.BluntStaminaDamageFactor).Float(), visual: false, source: user, with: meleeUid == user ? null : meleeUid, ignoreResist: ignoreResist);
                // END DeltaV
            }

            if (meleeUid == user)
            {
                AdminLogger.Add(LogType.MeleeHit,
                    LogImpact.Medium,
                    $"{ToPrettyString(user):actor} melee attacked (light) {ToPrettyString(target.Value):subject} using their hands and dealt {damageResult.GetTotal():damage} damage");
            }
            else
            {
                AdminLogger.Add(LogType.MeleeHit,
                    LogImpact.Medium,
                    $"{ToPrettyString(user):actor} melee attacked (light) {ToPrettyString(target.Value):subject} using {ToPrettyString(meleeUid):tool} and dealt {damageResult.GetTotal():damage} damage");
            }

        }

        _meleeSound.PlayHitSound(target.Value, user, GetHighestDamageSound(modifiedDamage, _protoManager), hitEvent.HitSoundOverride, component);

        if (damageResult?.GetTotal() > FixedPoint2.Zero)
        {
            DoDamageEffect(targets, user, targetXform);
        }
    }

    protected abstract void DoDamageEffect(List<EntityUid> targets, EntityUid? user,  TransformComponent targetXform);

    private bool DoHeavyAttack(EntityUid user, HeavyAttackEvent ev, EntityUid meleeUid, MeleeWeaponComponent component, ICommonSession? session)
    {
        // TODO: This is copy-paste as fuck with DoPreciseAttack
        if (!TryComp(user, out TransformComponent? userXform))
            return false;

        var targetMap = TransformSystem.ToMapCoordinates(GetCoordinates(ev.Coordinates));

        if (targetMap.MapId != userXform.MapID)
            return false;

        var userPos = TransformSystem.GetWorldPosition(userXform);
        var direction = targetMap.Position - userPos;
        var distance = Math.Min(component.Range, direction.Length());

        var damage = GetDamage(meleeUid, user, component);
        var resistanceBypass = GetResistanceBypass(meleeUid, user, component);
        var entities = GetEntityList(ev.Entities);

        if (entities.Count == 0)
        {
            if (meleeUid == user)
            {
                AdminLogger.Add(LogType.MeleeHit,
                    LogImpact.Low,
                    $"{ToPrettyString(user):actor} melee attacked (heavy) using their hands and missed");
            }
            else
            {
                AdminLogger.Add(LogType.MeleeHit,
                    LogImpact.Low,
                    $"{ToPrettyString(user):actor} melee attacked (heavy) using {ToPrettyString(meleeUid):tool} and missed");
            }
            var missEvent = new MeleeHitEvent(new List<EntityUid>(), user, meleeUid, damage, direction, GetCoordinates(ev.Coordinates)); // Goob edit
            RaiseLocalEvent(meleeUid, missEvent, true); // Goob station - broadcast

            // immediate audio feedback
            _meleeSound.PlaySwingSound(user, meleeUid, component);

            return true;
        }

        // Naughty input
        if (entities.Count > MaxTargets)
        {
            entities.RemoveRange(MaxTargets, entities.Count - MaxTargets);
        }

        // Validate client
        for (var i = entities.Count - 1; i >= 0; i--)
        {
            // Goob Fix Start
            var entity = entities[i];

            if (!entity.IsValid() || TerminatingOrDeleted(entity))
            {
                entities.RemoveAt(i);
                continue;
            }
            // Goob Fix End

            if (ArcRaySuccessful(entity,
                    userPos,
                    direction.ToWorldAngle(),
                    component.Angle,
                    distance,
                    userXform.MapID,
                    user,
                    session))
            {
                continue;
            }

            // Bad input
            entities.RemoveAt(i);
        }

        var targets = new List<EntityUid>();
        var damageQuery = GetEntityQuery<DamageableComponent>();

        foreach (var entity in entities)
        {
            if (entity == user ||
                !damageQuery.HasComponent(entity))
                continue;

            // Goobstation start
            var beforeEvent = new BeforeHarmfulActionEvent(user, HarmfulActionType.Harm);
            RaiseLocalEvent(entity, beforeEvent);
            if (beforeEvent.Cancelled)
                continue;
            // Goobstation end

            targets.Add(entity);
        }

        // Sawmill.Debug($"Melee damage is {damage.Total} out of {component.Damage.Total}");

        // Raise event before doing damage so we can cancel damage if the event is handled
        var hitEvent = new MeleeHitEvent(targets, user, meleeUid, damage, direction, GetCoordinates(ev.Coordinates)); // Goob edit
        RaiseLocalEvent(meleeUid, hitEvent, true); // Goob station - broadcast

        if (hitEvent.Handled)
            return true;

        var weapon = GetEntity(ev.Weapon);

        Interaction.DoContactInteraction(user, weapon);

        // For stuff that cares about it being attacked.
        foreach (var target in targets)
        {
            // We skip weapon -> target interaction, as forensics system applies DNA on hit

            // If the user is using a long-range weapon, this probably shouldn't be happening? But I'll interpret melee as a
            // somewhat messy scuffle. See also, light attacks.
            Interaction.DoContactInteraction(user, target);
        }

        var appliedDamage = new DamageSpecifier();
        var random = new System.Random((int) Timing.CurTick.Value); // Goobstation

        for (var i = targets.Count - 1; i >= 0; i--)
        {
            var entity = targets[i];
            // We raise an attack attempt here as well,
            // primarily because this was an untargeted wideswing: if a subscriber to that event cared about
            // the potential target (such as for pacifism), they need to be made aware of the target here.
            // In that case, just continue.
            if (!Blocker.CanAttack(user, entity, (weapon, component)))
            {
                targets.RemoveAt(i);
                continue;
            }

            var attackedEvent = new AttackedEvent(meleeUid, user, GetCoordinates(ev.Coordinates));
            RaiseLocalEvent(entity, attackedEvent);
            var modifiedDamage = DamageSpecifier.ApplyModifierSets(damage + hitEvent.BonusDamage + attackedEvent.BonusDamage, hitEvent.ModifiersList);
            modifiedDamage = DamageSpecifier.ApplyModifierSets(modifiedDamage, attackedEvent.ModifiersList); // Goobstation
            foreach (var type in modifiedDamage.DamageDict.Keys) // Goobstation
            {
                if (!modifiedDamage.WoundSeverityMultipliers.TryAdd(type, component.HeavyAttackWoundMultiplier))
                    modifiedDamage.WoundSeverityMultipliers[type] *= component.HeavyAttackWoundMultiplier;
            }
            var damageResult = Damageable.TryChangeDamage(entity, modifiedDamage, origin: user, ignoreResistances: resistanceBypass, partMultiplier: component.HeavyPartDamageMultiplier); // Shitmed Change
            var comboEv = new ComboAttackPerformedEvent(user, entity, meleeUid, ComboAttackType.HarmLight);
            RaiseLocalEvent(user, comboEv);

            if (damageResult != null && damageResult.GetTotal() > FixedPoint2.Zero)
            {
                // If the target has stamina and is taking blunt damage, they should also take stamina damage based on their blunt to stamina factor
                if (damageResult.DamageDict.TryGetValue("Blunt", out var bluntDamage))
                {
                    //DeltaV - No Stamina Resistance Doubledipping, unless we want to.
                    var ignoreResist = true;
                    if (TryComp<StaminaResistanceComponent>(entity, out var staminaResist))
                        ignoreResist = !staminaResist.MeleeResistance;

                    _stamina.TakeStaminaDamage(entity, (bluntDamage * component.BluntStaminaDamageFactor).Float(), visual: false, source: user, with: meleeUid == user ? null : meleeUid, ignoreResist: ignoreResist);
                    // END DeltaV
                }

                appliedDamage += damageResult;

                if (meleeUid == user)
                {
                    AdminLogger.Add(LogType.MeleeHit,
                        LogImpact.Medium,
                        $"{ToPrettyString(user):actor} melee attacked (heavy) {ToPrettyString(entity):subject} using their hands and dealt {damageResult.GetTotal():damage} damage");
                }
                else
                {
                    AdminLogger.Add(LogType.MeleeHit,
                        LogImpact.Medium,
                        $"{ToPrettyString(user):actor} melee attacked (heavy) {ToPrettyString(entity):subject} using {ToPrettyString(meleeUid):tool} and dealt {damageResult.GetTotal():damage} damage");
                }
            }
        }

        if (entities.Count != 0)
        {
            var target = entities.First();
            _meleeSound.PlayHitSound(target, user, GetHighestDamageSound(appliedDamage, _protoManager), hitEvent.HitSoundOverride, component);
        }

        if (appliedDamage.GetTotal() > FixedPoint2.Zero)
        {
            DoDamageEffect(targets, user, Transform(targets[0]));
        }

        // goob edit - stunmeta
        if (TryComp<StaminaComponent>(user, out var stamina) && entities.Count != 0)
            // make it not immediate to prevent annoying stamcrits
            _stamina.TakeStaminaDamage(user, component.HeavyStaminaCost * (entities.Count - 1), stamina, visual: false, immediate: false);

        return true;
    }

    public HashSet<EntityUid> ArcRayCast(Vector2 position, Angle angle, Angle arcWidth, float range, MapId mapId, EntityUid ignore) // Goob edit
    {
        // TODO: This is pretty sucky.
        var widthRad = arcWidth;
        var increments = 1 + 35 * (int) Math.Ceiling(widthRad / (2 * Math.PI));
        var increment = widthRad / increments;
        var baseAngle = angle - widthRad / 2;

        var resSet = new HashSet<EntityUid>();

        for (var i = 0; i < increments; i++)
        {
            var castAngle = new Angle(baseAngle + increment * i);
            var res = _physics.IntersectRay(mapId,
                new CollisionRay(position,
                    castAngle.ToWorldVec(),
                    AttackMask),
                range,
                ignore,
                false)
                .Where(x => !_tag.HasTag(x.HitEntity, "WideSwingIgnore")) // Goobstation
                .ToList();

            if (res.Count != 0)
            {
                // If there's exact distance overlap, we simply have to deal with all overlapping objects to avoid selecting randomly.
                var resChecked = res.Where(x => x.Distance.Equals(res[0].Distance));
                foreach (var r in resChecked)
                {
                    if (Interaction.InRangeUnobstructed(ignore, r.HitEntity, range + 0.1f, overlapCheck: false))
                        resSet.Add(r.HitEntity);
                }
            }
        }

        return resSet;
    }

    protected virtual bool ArcRaySuccessful(EntityUid targetUid,
        Vector2 position,
        Angle angle,
        Angle arcWidth,
        float range,
        MapId mapId,
        EntityUid ignore,
        ICommonSession? session)
    {
        // Only matters for server.
        return true;
    }


    public static string? GetHighestDamageSound(DamageSpecifier modifiedDamage, IPrototypeManager protoManager)
    {
        var groups = modifiedDamage.GetDamagePerGroup(protoManager);

        // Use group if it's exclusive, otherwise fall back to type.
        if (groups.Count == 1)
        {
            return groups.Keys.First();
        }

        var highestDamage = FixedPoint2.Zero;
        string? highestDamageType = null;

        foreach (var (type, damage) in modifiedDamage.DamageDict)
        {
            if (damage <= highestDamage)
                continue;

            highestDamageType = type;
        }

        return highestDamageType;
    }

    private float CalculateDisarmChance(EntityUid disarmer, EntityUid disarmed, EntityUid? inTargetHand, CombatModeComponent disarmerComp)
    {
        if (HasComp<DisarmProneComponent>(disarmer))
            return 1.0f;

        if (HasComp<DisarmProneComponent>(disarmed))
            return 0.0f;

        var chance = 1 - disarmerComp.BaseDisarmFailChance;

        // Goob - Shove Rework disarm based on health & stamina
        chance *= Math.Clamp(
            _contests.StaminaContest(disarmer, disarmed)
            * _contests.HealthContest(disarmer, disarmed),
            0f,
            1f);

        if (inTargetHand != null && TryComp<DisarmMalusComponent>(inTargetHand, out var malus))
            chance *= 1 - malus.Malus; // Goob - Shove Rework edit

        if (TryComp<ShovingComponent>(disarmer, out var shoving))
            chance *= 1 + shoving.DisarmBonus; // Goob - Shove Rework edit

        return chance;

    }

    // Goob - Shove Rework shove stamina damage based on mass
    private float CalculateShoveStaminaDamage(EntityUid disarmer, EntityUid disarmed)
    {
        var baseStaminaDamage = TryComp<ShovingComponent>(disarmer, out var shoving) ? shoving.StaminaDamage : ShovingComponent.DefaultStaminaDamage;

        return baseStaminaDamage * _contests.MassContest(disarmer, disarmed);
    }

    protected virtual bool DoDisarm(EntityUid user,
        DisarmAttackEvent ev,
        EntityUid meleeUid,
        MeleeWeaponComponent component,
        ICommonSession? session) // Goobstation - Shove Rework
    {
        if (!ev.Target.HasValue)
            return false;

        var target = GetEntity(ev.Target.Value);

        if (Deleted(target))
            return false;

        if (user == target) // Goobstation
        {
            _meleeSound.PlaySwingSound(user, meleeUid, component);
            var selfComboEv = new ComboAttackPerformedEvent(user, user, meleeUid, ComboAttackType.Disarm);
            RaiseLocalEvent(user, selfComboEv);
            return false;
        }

        if (!TryComp<CombatModeComponent>(user, out var combatMode))
            return false;

        if (!InRange(user, target, component.Range, session))
            return false;

        // Goobstation start
        var beforeEvent = new BeforeHarmfulActionEvent(user, HarmfulActionType.Disarm);
        RaiseLocalEvent(target, beforeEvent);
        if (beforeEvent.Cancelled)
            return false;

        var comboEv = new ComboAttackPerformedEvent(user, target, meleeUid, ComboAttackType.Disarm);
        RaiseLocalEvent(user, comboEv);
        // Goobstation end

        PhysicalShove(user, target);
        Interaction.DoContactInteraction(user, target);

        if (MobState.IsIncapacitated(target))
            return true;

        if (!TryComp<PhysicsComponent>(target, out var targetPhysicsComponent))
            return false;

        if (!TryComp<HandsComponent>(target, out var targetHandsComponent))
        {
            if (!TryComp<StatusEffectsComponent>(target, out var status) ||
                !status.AllowedEffects.Contains("KnockedDown"))
            {
                return true;
            }
        }

        if (!InRange(user, target, component.Range, session))
        {
            return false;
        }

        EntityUid? inTargetHand = null;

        if (_hands.TryGetActiveItem(target, out var activeHeldEntity))
        {
            inTargetHand = activeHeldEntity.Value;
        }

        var attemptEvent = new DisarmAttemptEvent(target, user, inTargetHand);
        if (inTargetHand != null)
        {
            RaiseLocalEvent(inTargetHand.Value, ref attemptEvent);
        }

        RaiseLocalEvent(target, ref attemptEvent);

        if (attemptEvent.Cancelled)
            return true;

        var chance = CalculateDisarmChance(user, target, inTargetHand, combatMode);

        _audio.PlayPredicted(combatMode.DisarmSuccessSound,
            user, user,
            AudioParams.Default.WithVariation(0.025f).WithVolume(5f));
        AdminLogger.Add(LogType.DisarmedAction,
            $"{ToPrettyString(user):user} used disarm on {ToPrettyString(target):target}");

        var staminaDamage = CalculateShoveStaminaDamage(user, target);

        var eventArgs = new DisarmedEvent(target,user,chance)
        {
            StaminaDamage = staminaDamage,
        };
        RaiseLocalEvent(target, ref eventArgs);

        if (!eventArgs.Handled)
        {
            ShoveOrDisarmPopup(false);
            return true;
        }

        ShoveOrDisarmPopup(true);

        return true;

        // Goob - Shove Rework edit (moved to function)
        void ShoveOrDisarmPopup(bool disarm)
        {
            var filterOther = Filter.PvsExcept(user, entityManager: EntityManager);
            var msgPrefix = "disarm-action-";

            if (!disarm)
                msgPrefix = "disarm-action-shove-";

            var msgOther = Loc.GetString(
                msgPrefix + "popup-message-other-clients",
                ("performerName", Identity.Entity(user, EntityManager)),
                ("targetName", Identity.Entity(target, EntityManager)));

            var msgUser = Loc.GetString(msgPrefix + "popup-message-cursor", ("targetName", Identity.Entity(target, EntityManager)));

            PopupSystem.PopupPredicted(msgOther, target, null, filterOther, false);
            PopupSystem.PopupClient(msgUser, user);
        }
    }

    private void DoLungeAnimation(EntityUid user, EntityUid weapon, Angle angle, MapCoordinates coordinates, float length, string? animation, Angle spriteRotation, bool flipAnimation)
    {
        // TODO: Assert that offset eyes are still okay.
        if (!TryComp(user, out TransformComponent? userXform))
            return;

        var invMatrix = TransformSystem.GetInvWorldMatrix(userXform);
        var localPos = Vector2.Transform(coordinates.Position, invMatrix);

        if (localPos.LengthSquared() <= 0f)
            return;

        localPos = userXform.LocalRotation.RotateVec(localPos);

        // We'll play the effect just short visually so it doesn't look like we should be hitting but actually aren't.
        const float bufferLength = 0.2f;
        var visualLength = length - bufferLength;

        if (localPos.Length() > visualLength)
            localPos = localPos.Normalized() * visualLength;

        DoLunge(user, weapon, angle, localPos, animation, spriteRotation, flipAnimation);
    }

    private void PhysicalShove(EntityUid user, EntityUid target)
    {
        var force = _shoveRange * _contests.MassContest(user, target, rangeFactor: _shoveMass);

        var userPos = TransformSystem.ToMapCoordinates(user.ToCoordinates()).Position;
        var targetPos = TransformSystem.ToMapCoordinates(target.ToCoordinates()).Position;
        var pushVector = (targetPos - userPos).Normalized() * force;
        var animated = HasComp<ItemComponent>(target);
        _throwing.TryThrow(target, pushVector, force * _shoveSpeed, animated: animated);
    }

    public abstract void DoLunge(EntityUid user, EntityUid weapon, Angle angle, Vector2 localPos, string? animation, Angle spriteRotation, bool flipAnimation, bool predicted = true);

    /// <summary>
    /// Used to update the MeleeWeapon component on item toggle.
    /// </summary>
    private void OnItemToggle(EntityUid uid, ItemToggleMeleeWeaponComponent itemToggleMelee, ItemToggledEvent args)
    {
        if (!TryComp(uid, out MeleeWeaponComponent? meleeWeapon))
            return;

        if (args.Activated)
        {
            if (itemToggleMelee.ActivatedDamage != null)
            {
                //Setting deactivated damage to the weapon's regular value before changing it.
                itemToggleMelee.DeactivatedDamage ??= meleeWeapon.Damage;
                meleeWeapon.Damage = itemToggleMelee.ActivatedDamage;
                DirtyField(uid, meleeWeapon, nameof(MeleeWeaponComponent.Damage));
            }

            if (meleeWeapon.HitSound?.Equals(itemToggleMelee.ActivatedSoundOnHit) != true)
            {
                meleeWeapon.HitSound = itemToggleMelee.ActivatedSoundOnHit;
                DirtyField(uid, meleeWeapon, nameof(MeleeWeaponComponent.HitSound));
            }

            if (itemToggleMelee.ActivatedSoundOnHitNoDamage != null)
            {
                //Setting the deactivated sound on no damage hit to the weapon's regular value before changing it.
                itemToggleMelee.DeactivatedSoundOnHitNoDamage ??= meleeWeapon.NoDamageSound;
                meleeWeapon.NoDamageSound = itemToggleMelee.ActivatedSoundOnHitNoDamage;
                DirtyField(uid, meleeWeapon, nameof(MeleeWeaponComponent.NoDamageSound));
            }

            if (itemToggleMelee.ActivatedSoundOnSwing != null)
            {
                //Setting the deactivated sound on no damage hit to the weapon's regular value before changing it.
                itemToggleMelee.DeactivatedSoundOnSwing ??= meleeWeapon.SwingSound;
                meleeWeapon.SwingSound = itemToggleMelee.ActivatedSoundOnSwing;
                DirtyField(uid, meleeWeapon, nameof(MeleeWeaponComponent.SwingSound));
            }

            if (itemToggleMelee.DeactivatedSecret)
            {
                meleeWeapon.Hidden = false;
            }
        }
        else
        {
            if (itemToggleMelee.DeactivatedDamage != null)
            {
                meleeWeapon.Damage = itemToggleMelee.DeactivatedDamage;
                DirtyField(uid, meleeWeapon, nameof(MeleeWeaponComponent.Damage));
            }

            meleeWeapon.HitSound = itemToggleMelee.DeactivatedSoundOnHit;
            DirtyField(uid, meleeWeapon, nameof(MeleeWeaponComponent.HitSound));

            if (itemToggleMelee.DeactivatedSoundOnHitNoDamage != null)
            {
                meleeWeapon.NoDamageSound = itemToggleMelee.DeactivatedSoundOnHitNoDamage;
                DirtyField(uid, meleeWeapon, nameof(MeleeWeaponComponent.NoDamageSound));
            }

            if (itemToggleMelee.DeactivatedSoundOnSwing != null)
            {
                meleeWeapon.SwingSound = itemToggleMelee.DeactivatedSoundOnSwing;
                DirtyField(uid, meleeWeapon, nameof(MeleeWeaponComponent.SwingSound));
            }

            if (itemToggleMelee.DeactivatedSecret)
            {
                meleeWeapon.Hidden = true;
            }
        }
    }
}
