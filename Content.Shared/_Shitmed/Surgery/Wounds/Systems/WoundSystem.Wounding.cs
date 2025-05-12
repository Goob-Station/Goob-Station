// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Shitmed.CCVar;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Weapons.Melee.Events;
using Content.Shared._Shitmed.Weapons.Ranged.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Gibbing.Events;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Standing;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

public sealed partial class WoundSystem
{
    private const string WoundContainerId = "Wounds";
    private const string BoneContainerId = "Bone";
    private void InitWounding()
    {
        SubscribeLocalEvent<WoundableComponent, ComponentInit>(OnWoundableInit);
        SubscribeLocalEvent<WoundableComponent, MapInitEvent>(OnWoundableMapInit);
        SubscribeLocalEvent<WoundableComponent, EntInsertedIntoContainerMessage>(OnWoundableInserted);
        SubscribeLocalEvent<WoundableComponent, EntRemovedFromContainerMessage>(OnWoundableRemoved);
        SubscribeLocalEvent<WoundComponent, EntGotInsertedIntoContainerMessage>(OnWoundInserted);
        SubscribeLocalEvent<WoundComponent, EntGotRemovedFromContainerMessage>(OnWoundRemoved);
        SubscribeLocalEvent<WoundableComponent, AttemptEntityContentsGibEvent>(OnWoundableContentsGibAttempt);
        SubscribeLocalEvent<WoundComponent, WoundSeverityChangedEvent>(OnWoundSeverityChanged);
        SubscribeLocalEvent<WoundableComponent, WoundableSeverityChangedEvent>(OnWoundableSeverityChanged);
        SubscribeLocalEvent<WoundableComponent, BeforeDamageChangedEvent>(DudeItsJustLikeMatrix);
        SubscribeLocalEvent<WoundableComponent, WoundHealAttemptOnWoundableEvent>(HealWoundsOnWoundableAttempt);
        SubscribeLocalEvent<WoundableComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<WoundableComponent, GetDoAfterDelayMultiplierEvent>(OnGetDoAfterDelayMultiplier);
        SubscribeLocalEvent<WoundableComponent, AttemptHandsMeleeEvent>(OnAttemptHandsMelee);
        SubscribeLocalEvent<WoundableComponent, AttemptHandsShootEvent>(OnAttemptHandsShoot);
    }

    #region Event Handling

    private void OnWoundableInit(EntityUid uid, WoundableComponent comp, ComponentInit args)
    {
        comp.RootWoundable = uid;
        comp.Wounds = _container.EnsureContainer<Container>(uid, WoundContainerId);
        comp.Bone = _container.EnsureContainer<Container>(uid, BoneContainerId);

        if (TryComp<BodyComponent>(uid, out var body) && _timing.IsFirstTimePredicted)
        {
            body.HealAt = _timing.CurTime + TimeSpan.FromSeconds(1f / _medicalHealingTickrate);
            _healQueue.Enqueue((uid, comp));
        }
    }

    private void OnWoundableMapInit(EntityUid uid, WoundableComponent comp, MapInitEvent args)
    {
        var bone = Spawn(comp.BoneEntity);
        if (!TryComp<BoneComponent>(bone, out var boneComp))
            return;

        _transform.SetParent(bone, uid);
        _container.Insert(bone, comp.Bone);
        boneComp.BoneWoundable = uid;
        Dirty(uid, comp);
    }

    private void OnWoundInserted(EntityUid uid, WoundComponent comp, EntGotInsertedIntoContainerMessage args)
    {
        if (comp.HoldingWoundable == EntityUid.Invalid)
            return;

        var parentWoundable = Comp<WoundableComponent>(comp.HoldingWoundable);

        if (!TryComp<WoundableComponent>(parentWoundable.RootWoundable, out var woundableRoot))
            return;

        var ev = new WoundAddedEvent(comp, parentWoundable, woundableRoot);
        RaiseLocalEvent(uid, ref ev);

        var ev1 = new WoundAddedEvent(comp, parentWoundable, woundableRoot);
        RaiseLocalEvent(comp.HoldingWoundable, ref ev1);

        var bodyPart = Comp<BodyPartComponent>(comp.HoldingWoundable);
        if (bodyPart.Body.HasValue)
        {
            var ev2 = new WoundAddedOnBodyEvent((uid, comp), parentWoundable, woundableRoot);
            RaiseLocalEvent(bodyPart.Body.Value, ref ev2);
        }
    }

    private void OnWoundRemoved(EntityUid woundableEntity, WoundComponent wound, EntGotRemovedFromContainerMessage args)
    {
        if (wound.HoldingWoundable == EntityUid.Invalid)
            return;

        if (!TryComp(wound.HoldingWoundable, out WoundableComponent? oldParentWoundable) ||
            !TryComp(oldParentWoundable.RootWoundable, out WoundableComponent? oldWoundableRoot))
            return;

        wound.HoldingWoundable = EntityUid.Invalid;

        var ev = new WoundRemovedEvent(wound, oldParentWoundable, oldWoundableRoot);
        RaiseLocalEvent(wound.HoldingWoundable, ref ev);

        if (_net.IsServer && !IsClientSide(woundableEntity))
            QueueDel(woundableEntity);
    }

    private void OnWoundableInserted(EntityUid parentEntity, WoundableComponent parentWoundable, EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<WoundableComponent>(args.Entity, out var childWoundable)
            || !_net.IsServer)
            return;

        InternalAddWoundableToParent(parentEntity, args.Entity, parentWoundable, childWoundable);
    }

    private void OnWoundableRemoved(EntityUid parentEntity, WoundableComponent parentWoundable, EntRemovedFromContainerMessage args)
    {
        if (!TryComp<WoundableComponent>(args.Entity, out var childWoundable)
            || !_net.IsServer)
            return;

        InternalRemoveWoundableFromParent(parentEntity, args.Entity, parentWoundable, childWoundable);
    }

    private void OnWoundableSeverityChanged(EntityUid uid, WoundableComponent component, WoundableSeverityChangedEvent args)
    {
        if (TerminatingOrDeleted(uid)
            || args.NewSeverity != WoundableSeverity.Loss
            || component.Bone.ContainedEntities.FirstOrNull() is { } bone
            && TryComp(bone, out BoneComponent? boneComp)
            && boneComp.BoneSeverity != BoneSeverity.Broken)
            return;

        if (IsWoundableRoot(uid, component))
        {
            DestroyWoundable(uid, uid, component);
            // We can call DestroyWoundable instead of ProcessBodyPartLoss, because the body will be gibbed, and we may not process body part loss.
        }
        else
        {
            if (component.ParentWoundable != null && Comp<BodyPartComponent>(uid).Body != null)
            {
                DestroyWoundable(component.ParentWoundable.Value, uid, component);
            }
            else
            {
                // it will be destroyed.
                DestroyWoundable(uid, uid, component);
            }
        }
    }

    private void OnWoundableContentsGibAttempt(EntityUid uid, WoundableComponent comp, ref AttemptEntityContentsGibEvent args)
    {
        if (args.ExcludedContainers == null)
            args.ExcludedContainers = new List<string> { WoundContainerId, BoneContainerId };
        else
            args.ExcludedContainers.AddRange(new List<string> { WoundContainerId, BoneContainerId });
    }

    private void DudeItsJustLikeMatrix(EntityUid uid, WoundableComponent comp, ref BeforeDamageChangedEvent args)
    {
        return;
        /* The world is not ready for you yet...

        if (!args.CanBeCancelled
            || args.Damage.GetTotal() <= 0)
            return;

        var chance = comp.DodgeChance;

        var bodyPart = Comp<BodyPartComponent>(uid);
        if (args.Origin != null)
        {
            if (bodyPart.Body != null)
            {
                var bodyTransform = _transform.GetWorldPosition(bodyPart.Body.Value);
                var originTransform = _transform.GetWorldPosition(args.Origin.Value);

                var distance = (originTransform - bodyTransform).Length();
                if (distance < _cfg.GetCVar(SurgeryCVars.DodgeDistanceChance) * 2)
                {
                    chance = 0;
                }
                else
                {
                    var additionalChance =
                        distance
                        / _cfg.GetCVar(SurgeryCVars.DodgeDistanceChance)
                        * _cfg.GetCVar(SurgeryCVars.DodgeDistanceChange);
                    chance += additionalChance;
                }
            }
        }

        if (!_random.Prob(Math.Clamp((float) chance, 0, 1)))
            return;

        if (bodyPart.Body.HasValue)
        {
            // Critted or dead people of course can't dodge for shit.
            if (!_mobState.IsAlive(bodyPart.Body.Value))
                return;

            _popup.PopupEntity(Loc.GetString("woundable-dodged", ("entity", bodyPart.Body.Value)), bodyPart.Body.Value, PopupType.Medium);
        }

        args.Cancelled = true;*/
    }

    private void HealWoundsOnWoundableAttempt(Entity<WoundableComponent> woundable, ref WoundHealAttemptOnWoundableEvent args)
    {
        if (woundable.Comp.WoundableSeverity == WoundableSeverity.Loss)
            args.Cancelled = true;
    }

    private void OnWoundSeverityChanged(EntityUid wound, WoundComponent woundComponent, WoundSeverityChangedEvent args)
    {
        if (args.NewSeverity != WoundSeverity.Healed)
            return;

        TryMakeScar(wound, out _, woundComponent);
        RemoveWound(wound, woundComponent);
    }

    private void OnDamageChanged(EntityUid uid, WoundableComponent component, ref DamageChangedEvent args)
    {
        // Skip if there was no damage delta or if wounds aren't allowed
        if (args.DamageDelta == null
            || !component.AllowWounds
            || !_timing.IsFirstTimePredicted)
            return;

        // Create or update wounds based on damage changes
        foreach (var (damageType, damageValue) in args.DamageDelta.DamageDict)
        {
            if (damageValue == 0)
                continue; // Only create wounds for damage or healing

            if (damageValue < 0)
            {
                TryHealWoundsOnWoundable(uid, -damageValue, damageType, out var healed, component, ignoreBlockers: args.IgnoreBlockers);
            }
            else
            {
                // Only create wound if it's a valid damage type for wounds
                if (!IsWoundPrototypeValid(damageType))
                    continue;

                TryInduceWound(uid, damageType, damageValue, out _, component);
            }
        }

        // Update woundable integrity based on new damage
        UpdateWoundableIntegrity(uid, component);
        CheckWoundableSeverityThresholds(uid, component);
    }

    private void OnGetDoAfterDelayMultiplier(EntityUid uid, WoundableComponent component, ref GetDoAfterDelayMultiplierEvent args)
    {
        if (component.WoundableIntegrity > 50)
            return;

        args.Multiplier *= (float) (component.WoundableIntegrity / component.IntegrityCap);
    }

    private void OnAttemptHandsMelee(EntityUid uid, WoundableComponent component, ref AttemptHandsMeleeEvent args)
    {
        if (component.WoundableIntegrity > 25
            || args.Handled
            || !TryComp(uid, out BodyPartComponent? bodyPart)
            || bodyPart.Body is not { } body)
            return;

        if (TryFumble("arm-fumble", new SoundPathSpecifier("/Audio/Effects/slip.ogg"), body, 0.20f))
        {
            args.Handled = true;
            args.Cancel();
        }
    }

    private void OnAttemptHandsShoot(EntityUid uid, WoundableComponent component, ref AttemptHandsShootEvent args)
    {
        if (component.WoundableIntegrity > 25
            || args.Handled
            || !TryComp(uid, out BodyPartComponent? bodyPart)
            || bodyPart.Body is not { } body)
            return;

        if (TryFumble("arm-fumble", new SoundPathSpecifier("/Audio/Effects/slip.ogg"), body, 0.20f))
            args.Handled = true;
    }

    #endregion

    #region Public API

    public DamageGroupPrototype? GetDamageGroupByType(string id)
    {
        return (from @group in _prototype.EnumeratePrototypes<DamageGroupPrototype>()
                where @group.DamageTypes.Contains(id)
                select @group).FirstOrDefault();
    }

    public bool TryInduceWounds(
        EntityUid uid,
        DamageSpecifier damage,
        out List<Entity<WoundComponent>> woundsInduced,
        WoundableComponent? woundable = null)
    {
        woundsInduced = new List<Entity<WoundComponent>>();
        if (!Resolve(uid, ref woundable))
            return false;

        foreach (var woundToInduce in damage.DamageDict)
        {
            if (!TryInduceWound(uid, woundToInduce.Key, woundToInduce.Value, out var woundInduced, woundable))
                return false;

            woundsInduced.Add(woundInduced.Value);
        }

        return true;
    }

    public bool TryInduceWound(
        EntityUid uid,
        string woundId,
        FixedPoint2 severity,
        [NotNullWhen(true)] out Entity<WoundComponent>? woundInduced,
        WoundableComponent? woundable = null)
    {
        woundInduced = null;
        if (!Resolve(uid, ref woundable))
            return false;

        if (TryContinueWound(uid, woundId, severity, out woundInduced, woundable))
            return true;

        return TryCreateWound(
            uid,
            woundId,
            severity,
            out woundInduced,
            (from @group in _prototype.EnumeratePrototypes<DamageGroupPrototype>()
                where @group.DamageTypes.Contains(woundId)
                select @group).FirstOrDefault(),
            woundable);
    }

    /// <summary>
    /// Opens a new wound on a requested woundable.
    /// </summary>
    /// <param name="uid">UID of the woundable (body part).</param>
    /// <param name="woundProtoId">Wound prototype.</param>
    /// <param name="severity">Severity for wound to apply.</param>
    /// <param name="woundCreated">The wound that was created</param>
    /// <param name="damageGroup">Damage group.</param>
    /// <param name="woundable">Woundable component.</param>
    public bool TryCreateWound(
        EntityUid uid,
        string woundProtoId,
        FixedPoint2 severity,
        [NotNullWhen(true)] out Entity<WoundComponent>? woundCreated,
        DamageGroupPrototype? damageGroup,
        WoundableComponent? woundable = null)
    {
        woundCreated = null;

        if (!IsWoundPrototypeValid(woundProtoId)
            || !Resolve(uid, ref woundable))
            return false;

        var wound = Spawn(woundProtoId);
        if (AddWound(uid, wound, severity, damageGroup))
        {
            woundCreated = (wound, Comp<WoundComponent>(wound));
        }
        else
        {
            // The wound failed some important checks, and we cannot let an invalid wound to be spawned!
            if (_net.IsServer && !IsClientSide(wound))
                QueueDel(wound);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Continues wound with specific type, if there's any. Adds severity to it basically.
    /// </summary>
    /// <param name="uid">Woundable entity's UID.</param>
    /// <param name="id">Wound entity's ID.</param>
    /// <param name="severity">Severity to apply.</param>
    /// <param name="woundContinued">The wound the severity was applied to, if any</param>
    /// <param name="woundable">Woundable for wound to add.</param>
    /// <returns>Returns true, if wound was continued.</returns>
    public bool TryContinueWound(
        EntityUid uid,
        string id,
        FixedPoint2 severity,
        [NotNullWhen(true)] out Entity<WoundComponent>? woundContinued,
        WoundableComponent? woundable = null)
    {
        woundContinued = null;
        if (!IsWoundPrototypeValid(id)
            || !Resolve(uid, ref woundable))
            return false;

        var proto = _prototype.Index(id);
        foreach (var wound in GetWoundableWounds(uid, woundable))
        {
            if (proto.ID != wound.Comp.DamageType)
                continue;

            ApplyWoundSeverity(wound, severity, wound);
            woundContinued = wound;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to create a scar on a woundable entity. Takes a scar prototype from WoundComponent.
    /// </summary>
    /// <param name="wound">The wound entity, from which the scar will be made.</param>
    /// <param name="scarWound">The result scar wound, if created.</param>
    /// <param name="woundComponent">The WoundComponent representing a specific wound.</param>
    public bool TryMakeScar(EntityUid wound,
        [NotNullWhen(true)] out Entity<WoundComponent>? scarWound,
        WoundComponent? woundComponent = null)
    {
        scarWound = null;
        if (!Resolve(wound, ref woundComponent))
            return false;

        if (!_random.Prob(_cfg.GetCVar(SurgeryCVars.WoundScarChance)))
            return false;

        if (woundComponent.ScarWound == null || woundComponent.IsScar)
            return false;

        if (!TryCreateWound(woundComponent.HoldingWoundable,
                woundComponent.ScarWound,
                0.1f,
                out var createdWound,
                woundComponent.DamageGroup))
            return false;

        scarWound = createdWound;
        return true;
    }

    /// <summary>
    /// Sets severity of a wound.
    /// </summary>
    /// <param name="uid">UID of the wound.</param>
    /// <param name="severity">Severity to set.</param>
    /// <param name="wound">Wound to which severity is applied.</param>
    public void SetWoundSeverity(EntityUid uid, FixedPoint2 severity, WoundComponent? wound = null)
    {
        if (!Resolve(uid, ref wound))
            return;

        var bodyPart = Comp<BodyPartComponent>(wound.HoldingWoundable);
        var old = wound.WoundSeverityPoint;
        wound.WoundSeverityPoint =
            FixedPoint2.Clamp(ApplySeverityModifiers(wound.HoldingWoundable, severity), 0, _cfg.GetCVar(SurgeryCVars.MaxWoundSeverity));

        if (wound.WoundSeverityPoint != old)
        {
            var ev = new WoundSeverityPointChangedEvent(wound, old, wound.WoundSeverityPoint);
            RaiseLocalEvent(uid, ref ev);

            var bodySeverity = FixedPoint2.Zero;

            if (bodyPart.Body.HasValue)
            {
                var rootPart = Comp<BodyComponent>(bodyPart.Body.Value).RootContainer.ContainedEntity;
                if (rootPart.HasValue)
                {
                    bodySeverity =
                        GetAllWoundableChildren(rootPart.Value)
                            .Aggregate(bodySeverity, (current, woundable) => current + GetWoundableSeverityPoint(woundable, woundable));
                }

                var ev1 = new WoundSeverityPointChangedOnBodyEvent(
                    (uid, wound),
                    bodySeverity - (wound.WoundSeverityPoint - old),
                    bodySeverity);
                RaiseLocalEvent(bodyPart.Body.Value, ref ev1);
            }
        }

        CheckSeverityThresholds(uid, wound);
        Dirty(uid, wound);

        UpdateWoundableIntegrity(wound.HoldingWoundable);
        CheckWoundableSeverityThresholds(wound.HoldingWoundable);
    }

    /// <summary>
    /// Applies severity to a wound
    /// </summary>
    /// <param name="uid">UID of the wound.</param>
    /// <param name="severity">Severity to add.</param>
    /// <param name="wound">Wound to which severity is applied.</param>
    /// <param name="traumaList">Traumas to apply when applying severity.. Please use _trauma.RandomTraumaChance if you expect your thing to apply traumas.</param>
    public void ApplyWoundSeverity(
        EntityUid uid,
        FixedPoint2 severity,
        WoundComponent? wound = null)
    {
        if (!Resolve(uid, ref wound)
            || !TryComp(wound.HoldingWoundable, out BodyPartComponent? bodyPart))
            return;

        var old = wound.WoundSeverityPoint;
        var rawValue = severity > 0
            ? old + ApplySeverityModifiers(wound.HoldingWoundable, severity)
            : old + severity;

        wound.WoundSeverityPoint = FixedPoint2.Clamp(rawValue, 0, _cfg.GetCVar(SurgeryCVars.MaxWoundSeverity));

        if (wound.WoundSeverityPoint != old || rawValue > wound.WoundSeverityPoint)
        {
            // We keep track of this overflow variable to allow continuous damage on wounds that have been capped
            // i.e. slashing nonstop at a dead body to continue inflicting traumas.
            FixedPoint2? overflow = rawValue > wound.WoundSeverityPoint ? rawValue - wound.WoundSeverityPoint : null;
            var ev = new WoundSeverityPointChangedEvent(wound, old, wound.WoundSeverityPoint, overflow);
            RaiseLocalEvent(uid, ref ev);

            var bodySeverity = FixedPoint2.Zero;

            if (bodyPart.Body.HasValue)
            {
                var rootPart = Comp<BodyComponent>(bodyPart.Body.Value).RootContainer.ContainedEntity;
                if (rootPart.HasValue)
                {
                    bodySeverity =
                        GetAllWoundableChildren(rootPart.Value)
                            .Aggregate(bodySeverity,
                                (current, woundable) => current + GetWoundableSeverityPoint(woundable, woundable));
                }

                var ev1 = new WoundSeverityPointChangedOnBodyEvent(
                    (uid, wound),
                    bodySeverity - (wound.WoundSeverityPoint - old),
                    bodySeverity);
                RaiseLocalEvent(bodyPart.Body.Value, ref ev1);
            }
        }

        if (severity > 0
            && wound.MangleSeverity != null
            && HasWoundsExceedingMangleSeverity(wound.HoldingWoundable))
            _trauma.ApplyMangledTraumas(wound.HoldingWoundable, wound.Owner, severity);

        var holdingWoundable = wound.HoldingWoundable;
        CheckSeverityThresholds(uid, wound);

        UpdateWoundableIntegrity(holdingWoundable);
        CheckWoundableSeverityThresholds(holdingWoundable);
    }

    public FixedPoint2 ApplySeverityModifiers(
        EntityUid woundable,
        FixedPoint2 severity,
        WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component))
            return severity;

        if (component.SeverityMultipliers.Count == 0)
            return severity;

        var toMultiply =
            component.SeverityMultipliers.Sum(multiplier => (float) multiplier.Value.Change) / component.SeverityMultipliers.Count;
        return severity * toMultiply;
    }

    /// <summary>
    /// Applies severity multiplier to a wound.
    /// </summary>
    /// <param name="uid">UID of the woundable.</param>
    /// <param name="owner">UID of the multiplier owner.</param>
    /// <param name="change">The severity multiplier itself</param>
    /// <param name="identifier">A string to defy this multiplier from others.</param>
    /// <param name="component">Woundable to which severity multiplier is applied.</param>
    public bool TryAddWoundableSeverityMultiplier(
        EntityUid uid,
        EntityUid owner,
        FixedPoint2 change,
        string identifier,
        WoundableComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || component.Wounds == null
            || !_net.IsServer)
            return false;

        if (!component.SeverityMultipliers.TryAdd(owner, new WoundableSeverityMultiplier(change, identifier)))
            return false;

        foreach (var wound in component.Wounds.ContainedEntities)
            CheckSeverityThresholds(wound);

        UpdateWoundableIntegrity(uid, component);
        CheckWoundableSeverityThresholds(uid, component);

        return true;
    }

    /// <summary>
    /// Removes a multiplier from a woundable.
    /// </summary>
    /// <param name="uid">UID of the woundable.</param>
    /// <param name="identifier">Identifier of the said multiplier.</param>
    /// <param name="component">Woundable to which severity multiplier is applied.</param>
    public bool TryRemoveWoundableSeverityMultiplier(
        EntityUid uid,
        string identifier,
        WoundableComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || component.Wounds == null
            || !_net.IsServer)
            return false;

        foreach (var multiplier in component.SeverityMultipliers.Where(multiplier => multiplier.Value.Identifier == identifier))
        {
            if (!component.SeverityMultipliers.Remove(multiplier.Key, out _))
                return false;

            foreach (var wound in component.Wounds.ContainedEntities)
                CheckSeverityThresholds(wound);

            UpdateWoundableIntegrity(uid, component);
            CheckWoundableSeverityThresholds(uid, component);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Changes a multiplier's change in a specified woundable.
    /// </summary>
    /// <param name="uid">UID of the woundable.</param>
    /// <param name="identifier">Identifier of the said multiplier.</param>
    /// <param name="change">The new multiplier fixed point.</param>
    /// <param name="component">Woundable to which severity multiplier is applied.</param>
    public bool TryChangeWoundableSeverityMultiplier(
        EntityUid uid,
        string identifier,
        FixedPoint2 change,
        WoundableComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || component.Wounds == null
            || !_net.IsServer)
            return false;

        foreach (var multiplier in component.SeverityMultipliers.Where(multiplier => multiplier.Value.Identifier == identifier))
        {
            component.SeverityMultipliers.Remove(multiplier.Key, out var value);

            value.Change = change;
            component.SeverityMultipliers.Add(multiplier.Key, value);

            foreach (var wound in component.Wounds.ContainedEntities.ToList())
                CheckSeverityThresholds(wound);

            UpdateWoundableIntegrity(uid, component);
            CheckWoundableSeverityThresholds(uid, component);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Destroys an entity's body part if conditions are met.
    /// </summary>
    /// <param name="parentWoundableEntity">Parent of the woundable entity. Yes.</param>
    /// <param name="woundableEntity">The entity containing the vulnerable body part</param>
    /// <param name="woundableComp">Woundable component of woundableEntity.</param>
    public void DestroyWoundable(EntityUid parentWoundableEntity, EntityUid woundableEntity, WoundableComponent woundableComp)
    {
        var bodyPart = Comp<BodyPartComponent>(woundableEntity);
        if (bodyPart.Body == null)
        {
            DropWoundableOrgans(woundableEntity, woundableComp);
            if (_net.IsServer && !IsClientSide(woundableEntity))
                QueueDel(woundableEntity);
        }
        else
        {
            var key = bodyPart.ToHumanoidLayers();
            if (key == null)
                return;

            // if wounds amount somehow changes it triggers an enumeration error. owch
            woundableComp.AllowWounds = false;
            woundableComp.WoundableSeverity = WoundableSeverity.Loss;

            if (TryComp<TargetingComponent>(bodyPart.Body.Value, out var targeting))
            {
                targeting.BodyStatus = GetWoundableStatesOnBodyPainFeels(bodyPart.Body.Value);
                Dirty(bodyPart.Body.Value, targeting);

                if (_net.IsServer)
                    RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(bodyPart.Body.Value)), bodyPart.Body.Value);
            }

            _audio.PlayPvs(woundableComp.WoundableDestroyedSound, bodyPart.Body.Value);
            _appearance.SetData(woundableEntity,
                WoundableVisualizerKeys.Wounds,
                new WoundVisualizerGroupData(GetWoundableWounds(woundableEntity).Select(ent => GetNetEntity(ent)).ToList()));

            foreach (var wound in GetWoundableWounds(parentWoundableEntity))
            {
                if (MetaData(wound.Owner).EntityPrototype!.ID != "Blunt") //lol
                    continue;

                _trauma.AddTrauma(
                    parentWoundableEntity,
                    (parentWoundableEntity, Comp<WoundableComponent>(parentWoundableEntity)),
                    (wound.Owner, EnsureComp<TraumaInflicterComponent>(wound.Owner)),
                    TraumaType.Dismemberment,
                    15f,
                    targetType: (bodyPart.PartType, bodyPart.Symmetry));
                break;
            }

            Dirty(woundableEntity, woundableComp);

            if (IsWoundableRoot(woundableEntity, woundableComp))
            {
                /*DropWoundableOrgans(woundableEntity, woundableComp);
                DestroyWoundableChildren(woundableEntity, woundableComp);
                _body.GibBody(bodyPart.Body.Value);

                if (_net.IsServer)
                {
                    if (!IsClientSide(woundableEntity))
                        QueueDel(woundableEntity); // More blood for the blood God!

                    _body.GibBody(bodyPart.Body.Value);
                }
                }*/
            }
            else
            {
                if (!_container.TryGetContainingContainer(parentWoundableEntity, woundableEntity, out var container))
                    return;

                if (bodyPart.Body is not null
                    && TryComp<InventoryComponent>(bodyPart.Body, out var inventory) // Prevent error for non-humanoids
                    && _body.GetBodyPartCount(bodyPart.Body.Value, bodyPart.PartType) == 1
                    && _body.TryGetPartSlotContainerName(bodyPart.PartType, out var containerNames))
                {
                    foreach (var containerName in containerNames)
                    {
                        _inventory.DropSlotContents(bodyPart.Body.Value, containerName, inventory);
                    }
                }
                var bodyPartId = container.ID;

                // Prevent anomalous behaviour
                if (bodyPart.PartType is BodyPartType.Hand or BodyPartType.Arm)
                    _hands.TryDrop(bodyPart.Body!.Value, woundableEntity);

                DropWoundableOrgans(woundableEntity, woundableComp);
                DestroyWoundableChildren(woundableEntity, woundableComp);

                foreach (var wound in GetWoundableWounds(woundableEntity, woundableComp))
                    TransferWoundDamage(parentWoundableEntity, woundableEntity, wound);

                if (TryInduceWound(parentWoundableEntity, "Blunt", 15f, out var woundEnt))
                {
                    _trauma.AddTrauma(
                        parentWoundableEntity,
                        (parentWoundableEntity, Comp<WoundableComponent>(parentWoundableEntity)),
                        (woundEnt.Value.Owner, EnsureComp<TraumaInflicterComponent>(woundEnt.Value.Owner)),
                        TraumaType.Dismemberment,
                        15f);
                }

                foreach (var wound in GetWoundableWounds(parentWoundableEntity))
                {
                    if (!TryComp<BleedInflicterComponent>(wound, out var bleeds))
                        continue;

                    // Bleeding :3
                    bleeds.ScalingLimit += 6;
                }

                _body.DetachPart(parentWoundableEntity, bodyPartId.Remove(0, 15), woundableEntity);

                if (_net.IsServer && !IsClientSide(woundableEntity))
                    QueueDel(woundableEntity);
            }
        }
    }

    /// <summary>
    /// Amputates (not destroys) an entity's body part if conditions are met.
    /// </summary>
    /// <param name="parentWoundableEntity">Parent of the woundable entity. Yes.</param>
    /// <param name="woundableEntity">The entity containing the vulnerable body part</param>
    /// <param name="woundableComp">Woundable component of woundableEntity.</param>
    public void AmputateWoundable(EntityUid parentWoundableEntity, EntityUid woundableEntity, WoundableComponent? woundableComp = null)
    {
        if (!Resolve(woundableEntity, ref woundableComp))
            return;

        var bodyPart = Comp<BodyPartComponent>(parentWoundableEntity);
        if (!bodyPart.Body.HasValue
            || !woundableComp.CanRemove)
            return;

        _audio.PlayPvs(woundableComp.WoundableDelimbedSound, bodyPart.Body.Value);

        if (woundableComp.DamageOnAmputate != null
            && _body.TryGetRootPart(bodyPart.Body.Value, out var rootPart))
        {
            var target = _body.GetTargetBodyPart(rootPart);

            if (target != null)
                _damageable.TryChangeDamage(bodyPart.Body.Value, woundableComp.DamageOnAmputate, targetPart: target);
        }

        foreach (var wound in GetWoundableWounds(woundableEntity, woundableComp))
            TransferWoundDamage(parentWoundableEntity, woundableEntity, wound);

        foreach (var wound in GetWoundableWounds(parentWoundableEntity))
        {
            if (!TryComp<BleedInflicterComponent>(wound, out var bleeds))
                continue;

            bleeds.ScalingLimit += 6;
        }

        AmputateWoundableSafely(parentWoundableEntity, woundableEntity);

        if (!_net.IsServer)
            return;

        var direction = _random.NextAngle().ToWorldVec();
        var dropAngle = _random.NextFloat(0.8f, 1.2f);
        var worldRotation = _transform.GetWorldRotation(woundableEntity).ToVec();

        _throwing.TryThrow(
            woundableEntity,
            _random.NextAngle().ToWorldVec() * _random.NextFloat(0.8f, 5f),
            _random.NextFloat(0.5f, 1f),
            pushbackRatio: 0.3f
        );
    }

    /// <summary>
    /// Does whatever AmputateWoundable does, but does it without pain and the other mess.
    /// </summary>
    /// <param name="parentWoundableEntity">Parent of the woundable entity. Yes.</param>
    /// <param name="woundableEntity">The entity containing the vulnerable body part</param>
    /// <param name="woundableComp">Woundable component of woundableEntity.</param>
    public void AmputateWoundableSafely(EntityUid parentWoundableEntity,
        EntityUid woundableEntity,
        WoundableComponent? woundableComp = null)
    {
        if (!Resolve(woundableEntity, ref woundableComp)
            || !woundableComp.CanRemove)
            return;

        var bodyPart = Comp<BodyPartComponent>(parentWoundableEntity);

        if (!bodyPart.Body.HasValue
            || !_container.TryGetContainingContainer(parentWoundableEntity, woundableEntity, out var container))
            return;

        var bodyPartId = container.ID;
        woundableComp.WoundableSeverity = WoundableSeverity.Loss;

        if (TryComp<TargetingComponent>(bodyPart.Body.Value, out var targeting))
        {
            targeting.BodyStatus = GetWoundableStatesOnBodyPainFeels(bodyPart.Body.Value);
            Dirty(bodyPart.Body.Value, targeting);

            if (_net.IsServer)
                RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(bodyPart.Body.Value)), bodyPart.Body.Value);
        }

        var childBodyPart = Comp<BodyPartComponent>(woundableEntity);
        if (TryComp<InventoryComponent>(bodyPart.Body, out var inventory)
            && _body.GetBodyPartCount(bodyPart.Body.Value, bodyPart.PartType) == 1
            && _body.TryGetPartSlotContainerName(childBodyPart.PartType, out var containerNames))
        {
            foreach (var containerName in containerNames)
                _inventory.DropSlotContents(bodyPart.Body.Value, containerName, inventory);
        }

        if (childBodyPart.PartType is BodyPartType.Hand or BodyPartType.Arm)
            _hands.TryDrop(bodyPart.Body!.Value, woundableEntity);

        Dirty(woundableEntity, woundableComp);
        _appearance.SetData(woundableEntity,
            WoundableVisualizerKeys.Wounds,
            new WoundVisualizerGroupData(GetWoundableWounds(woundableEntity).Select(ent => GetNetEntity(ent)).ToList()));

        // Still does the funny popping, if the children are critted. for the funny :3
        DestroyWoundableChildren(woundableEntity, woundableComp);
        _body.DetachPart(parentWoundableEntity, bodyPartId.Remove(0, 15), woundableEntity);
        _trauma.UpdateBodyBoneAlert(woundableEntity);
    }

    #endregion

    #region Private API

    private void DropWoundableOrgans(EntityUid woundable, WoundableComponent? woundableComp)
    {
        if (!Resolve(woundable, ref woundableComp, false))
            return;

        foreach (var organ in _body.GetPartOrgans(woundable))
        {
            if (organ.Component.OrganSeverity == OrganSeverity.Normal)
            {
                // TODO: SFX for organs getting not destroyed, but thrown out
                _body.RemoveOrgan(organ.Id, organ.Component);
                var direction = _random.NextAngle().ToWorldVec();
                var dropAngle = _random.NextFloat(0.8f, 1.2f);
                var worldRotation = _transform.GetWorldRotation(organ.Id).ToVec();

                _throwing.TryThrow(
                    organ.Id,
                    _random.NextAngle().RotateVec(direction / dropAngle + worldRotation / 50),
                    0.5f * dropAngle * _random.NextFloat(-0.9f, 1.1f),
                    doSpin: false,
                    pushbackRatio: 0
                );
            }
            else
            {
                // Destroy it
                _trauma.TrySetOrganDamageModifier(
                    organ.Id,
                    organ.Component.OrganIntegrity * 100,
                    woundable,
                    "LETMETELLYOUHOWMUCHIVECOMETOHATEYOUSINCEIBEGANTOLIVE",
                    organ.Component);
            }
        }
    }

    private void TransferWoundDamage(
        EntityUid parent,
        EntityUid severed,
        EntityUid wound,
        WoundableComponent? woundableComp = null,
        WoundComponent? woundComp = null)
    {
        if (!Resolve(parent, ref woundableComp, false)
            || !Resolve(wound, ref woundComp, false))
            return;

        var bodyPart = Comp<BodyPartComponent>(severed);

        if (TryComp(parent, out DamageableComponent? parentDamageable)
            && TryComp(severed, out DamageableComponent? severedDamageable)
            && bodyPart.Body is not null
            && TryComp<BodyComponent>(bodyPart.Body.Value, out var body)
            && body.RootContainer.ContainedEntities.Count > 0)
        {
            var damage = severedDamageable.Damage;
            _damageable.TryChangeDamage(bodyPart.Body.Value,
                damage,
                targetPart: _body.GetTargetBodyPart(body.RootContainer.ContainedEntities.First()),
                ignoreResistances: true,
                interruptsDoAfters: false);
        }

        foreach (var woundEnt in GetWoundableWounds(parent, woundableComp))
        {
            if (woundEnt.Comp.DamageType != woundComp.DamageType)
                continue;

            var tourniquetable = EnsureComp<TourniquetableComponent>(woundEnt);
            tourniquetable.SeveredSymmetry = bodyPart.Symmetry;
            tourniquetable.SeveredPartType = bodyPart.PartType;
        }
    }

    /// <summary>
    /// Updates the woundable integrity based on the current damage
    /// </summary>
    public void UpdateWoundableIntegrity(EntityUid uid, WoundableComponent? component = null, DamageableComponent? damageable = null)
    {
        if (!Resolve(uid, ref component, false)
            || !Resolve(uid, ref damageable, false)
            || component.Wounds == null)
            return;

        // Calculate total damage on this part
        var damage =
            component.Wounds.ContainedEntities.Select(Comp<WoundComponent>)
            .Where(wound => !wound.IsScar)
            .Aggregate(FixedPoint2.Zero, (current, wound) => current + wound.WoundIntegrityDamage);

        var newIntegrity = FixedPoint2.Clamp(component.IntegrityCap - damage, 0, component.IntegrityCap);
        if (newIntegrity == component.WoundableIntegrity)
            return;

        var ev = new WoundableIntegrityChangedEvent(component.WoundableIntegrity, newIntegrity);
        RaiseLocalEvent(uid, ref ev);

        var bodySeverity = FixedPoint2.Zero;
        var bodyPart = Comp<BodyPartComponent>(uid);

        if (bodyPart.Body.HasValue)
        {
            var rootPart = Comp<BodyComponent>(bodyPart.Body.Value).RootContainer.ContainedEntity;
            if (rootPart.HasValue)
            {
                bodySeverity =
                    GetAllWoundableChildren(rootPart.Value)
                        .Aggregate(bodySeverity, (current, woundable) => current + GetWoundableIntegrityDamage(woundable, woundable));
            }

            var ev1 = new WoundableIntegrityChangedOnBodyEvent(
                (uid, component),
                bodySeverity - (component.WoundableIntegrity - newIntegrity),
                bodySeverity);
            RaiseLocalEvent(bodyPart.Body.Value, ref ev1);
        }
        component.WoundableIntegrity = newIntegrity;
        Dirty(uid, component);
    }

    private bool AddWound(
        EntityUid target,
        EntityUid wound,
        FixedPoint2 woundSeverity,
        DamageGroupPrototype? damageGroup,
        WoundableComponent? woundableComponent = null,
        WoundComponent? woundComponent = null)
    {
        if (!_net.IsServer
            || !Resolve(target, ref woundableComponent)
            || !Resolve(wound, ref woundComponent)
            || woundableComponent.Wounds == null
            || woundableComponent.Wounds.Contains(wound)
            || !_timing.IsFirstTimePredicted
            || woundSeverity <= _woundThresholds[WoundSeverity.Healed]
            || !woundableComponent.AllowWounds)
            return false;

        _transform.SetParent(wound, target);
        woundComponent.HoldingWoundable = target;
        woundComponent.DamageGroup = damageGroup;

        if (!_container.Insert(wound, woundableComponent.Wounds))
            return false;

        SetWoundSeverity(wound, woundSeverity, woundComponent);
        var woundMeta = MetaData(wound);
        var targetMeta = MetaData(target);

        //Log.Debug($"Wound: {woundMeta.EntityPrototype!.ID}({wound}) created on {targetMeta.EntityPrototype!.ID}({target})");

        Dirty(wound, woundComponent);
        Dirty(target, woundableComponent);

        return true;
    }

    private bool RemoveWound(EntityUid woundEntity, WoundComponent? wound = null)
    {
        if (!_timing.IsFirstTimePredicted)
            return false;

        if (!Resolve(woundEntity, ref wound, false)
            || !TryComp(wound.HoldingWoundable, out WoundableComponent? woundable))
            return false;

        //Log.Debug($"Wound: {MetaData(woundEntity).EntityPrototype!.ID}({woundEntity}) removed on {MetaData(wound.HoldingWoundable).EntityPrototype!.ID}({wound.HoldingWoundable})");

        UpdateWoundableIntegrity(wound.HoldingWoundable, woundable);
        CheckWoundableSeverityThresholds(wound.HoldingWoundable, woundable);

        _container.Remove(woundEntity, woundable.Wounds!, false, true);

        return true;
    }

    protected void InternalAddWoundableToParent(
        EntityUid parentEntity,
        EntityUid childEntity,
        WoundableComponent parentWoundable,
        WoundableComponent childWoundable)
    {
        parentWoundable.ChildWoundables.Add(childEntity);
        childWoundable.ParentWoundable = parentEntity;
        childWoundable.RootWoundable = parentWoundable.RootWoundable;

        FixWoundableRoots(childEntity, childWoundable);

        if (!TryComp<WoundableComponent>(parentWoundable.RootWoundable, out var woundableRoot))
            return;

        var woundableAttached = new WoundableAttachedEvent(parentEntity, parentWoundable);

        RaiseLocalEvent(childEntity, ref woundableAttached);

        foreach (var (woundId, wound) in GetAllWounds(childEntity, childWoundable))
        {
            var ev = new WoundAddedEvent(wound, parentWoundable, woundableRoot);
            RaiseLocalEvent(woundId, ref ev);

            var bodyPart = Comp<BodyPartComponent>(childEntity);
            if (bodyPart.Body.HasValue)
            {
                var ev2 = new WoundAddedOnBodyEvent((woundId, wound), parentWoundable, woundableRoot);
                RaiseLocalEvent(bodyPart.Body.Value, ref ev2);
            }
        }

        Dirty(childEntity, childWoundable);
    }

    protected void InternalRemoveWoundableFromParent(
        EntityUid parentEntity,
        EntityUid childEntity,
        WoundableComponent parentWoundable,
        WoundableComponent childWoundable)
    {
        if (TerminatingOrDeleted(childEntity)
            || TerminatingOrDeleted(parentEntity))
            return;

        parentWoundable.ChildWoundables.Remove(childEntity);
        childWoundable.ParentWoundable = null;
        childWoundable.RootWoundable = childEntity;

        FixWoundableRoots(childEntity, childWoundable);

        if (!TryComp<WoundableComponent>(parentWoundable.RootWoundable, out var oldWoundableRoot))
            return;

        var woundableDetached = new WoundableDetachedEvent(parentEntity, parentWoundable);

        RaiseLocalEvent(childEntity, ref woundableDetached);

        foreach (var (woundId, wound) in GetAllWounds(childEntity, childWoundable))
        {
            var ev = new WoundRemovedEvent(wound, childWoundable, oldWoundableRoot);
            RaiseLocalEvent(woundId, ref ev);

            var ev2 = new WoundRemovedEvent(wound, childWoundable, oldWoundableRoot);
            RaiseLocalEvent(childWoundable.RootWoundable, ref ev2);
        }

        Dirty(childEntity, childWoundable);
    }

    private void FixWoundableRoots(EntityUid targetEntity, WoundableComponent targetWoundable)
    {
        if (targetWoundable.ChildWoundables.Count == 0)
            return;

        foreach (var (childEntity, childWoundable) in GetAllWoundableChildren(targetEntity, targetWoundable))
        {
            childWoundable.RootWoundable = targetWoundable.RootWoundable;
            Dirty(childEntity, childWoundable);
        }

        Dirty(targetEntity, targetWoundable);
    }

    private void CheckSeverityThresholds(EntityUid wound, WoundComponent? component = null)
    {
        if (!Resolve(wound, ref component, false)
            || !_net.IsServer)
            return;

        var nearestSeverity = component.WoundSeverity;
        foreach (var (severity, value) in _woundThresholds.OrderByDescending(kv => kv.Value))
        {
            if (component.WoundSeverityPoint < value)
                continue;

            if (severity == WoundSeverity.Healed && component.WoundSeverityPoint > 0)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity != component.WoundSeverity)
        {
            var ev = new WoundSeverityChangedEvent(component.WoundSeverity, nearestSeverity);
            RaiseLocalEvent(wound, ref ev);
        }
        component.WoundSeverity = nearestSeverity;

        if (!TerminatingOrDeleted(component.HoldingWoundable))
        {
            _appearance.SetData(component.HoldingWoundable,
                WoundableVisualizerKeys.Wounds,
                new WoundVisualizerGroupData(GetWoundableWounds(component.HoldingWoundable).Select(ent => GetNetEntity(ent)).ToList()));
        }
    }

    /// <summary>
    /// Checks if the current integrity crosses any severity thresholds and updates accordingly
    /// </summary>
    private void CheckWoundableSeverityThresholds(EntityUid woundable, WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component, false))
            return;

        var nearestSeverity = component.WoundableSeverity;
        foreach (var (severity, value) in component.Thresholds.OrderByDescending(kv => kv.Value))
        {
            if (component.WoundableIntegrity >= component.IntegrityCap)
            {
                nearestSeverity = WoundableSeverity.Healthy;
                break;
            }

            if (component.WoundableIntegrity < value)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity != component.WoundableSeverity)
        {
            var ev = new WoundableSeverityChangedEvent(component.WoundableSeverity, nearestSeverity);
            RaiseLocalEvent(woundable, ref ev);
        }
        component.WoundableSeverity = nearestSeverity;

        Dirty(woundable, component);

        var bodyPart = Comp<BodyPartComponent>(woundable);
        if (bodyPart.Body == null)
            return;

        if (!TryComp<TargetingComponent>(bodyPart.Body.Value, out var targeting))
            return;

        targeting.BodyStatus = GetWoundableStatesOnBodyPainFeels(bodyPart.Body.Value);
        Dirty(bodyPart.Body.Value, targeting);

        if (_net.IsServer)
            RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(bodyPart.Body.Value)), bodyPart.Body.Value);

        _appearance.SetData(woundable,
            WoundableVisualizerKeys.Wounds,
            new WoundVisualizerGroupData(GetWoundableWounds(woundable).Select(ent => GetNetEntity(ent)).ToList()));
    }

    private bool TryFumble(string message, SoundPathSpecifier sound, EntityUid body, float odds)
    {
        var rand = new System.Random((int) _timing.CurTick.Value);
        if (rand.NextFloat() < odds)
        {
            _popup.PopupClient(Loc.GetString(message), body, PopupType.Medium);
            var ev = new DropHandItemsEvent();
            RaiseLocalEvent(body, ref ev, false);
            _audio.PlayPredicted(sound, body, body);
            return true;
        }
        return false;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Validates the wound prototype based on the given prototype ID.
    /// Checks if the specified prototype ID corresponds to a valid EntityPrototype in the collection,
    /// ensuring it contains the necessary WoundComponent.
    /// </summary>
    /// <param name="protoId">The prototype ID to be validated.</param>
    /// <returns>True if the wound prototype is valid, otherwise false.</returns>
    private bool IsWoundPrototypeValid(string protoId)
    {
        return _prototype.TryIndex<EntityPrototype>(protoId, out var woundPrototype)
               && woundPrototype.TryGetComponent<WoundComponent>(out _, _factory);
    }

    private void DestroyWoundableChildren(EntityUid woundableEntity, WoundableComponent? woundableComp = null)
    {
        if (!Resolve(woundableEntity, ref woundableComp, false))
            return;

        foreach (var child in woundableComp.ChildWoundables)
        {
            var childWoundable = Comp<WoundableComponent>(child);
            if (childWoundable.WoundableSeverity is WoundableSeverity.Critical)
            {
                DestroyWoundable(woundableEntity, child, childWoundable);
                continue;
            }

            AmputateWoundable(woundableEntity, child, childWoundable);
        }
    }

    public Dictionary<TargetBodyPart, WoundableSeverity> GetWoundableStatesOnBody(EntityUid body)
    {
        var result = new Dictionary<TargetBodyPart, WoundableSeverity>();

        foreach (var part in SharedTargetingSystem.GetValidParts())
        {
            result[part] = WoundableSeverity.Loss;
        }

        foreach (var (id, bodyPart) in _body.GetBodyChildren(body))
        {
            var target = _body.GetTargetBodyPart(bodyPart);
            if (target == null)
                continue;

            if (!TryComp<WoundableComponent>(id, out var woundable))
                continue;

            result[target.Value] = woundable.WoundableSeverity;
        }

        return result;
    }

    public Dictionary<TargetBodyPart, WoundableSeverity> GetDamageableStatesOnBody(EntityUid body)
    {
        var result = new Dictionary<TargetBodyPart, WoundableSeverity>();

        foreach (var part in SharedTargetingSystem.GetValidParts())
        {
            result[part] = WoundableSeverity.Loss;
        }

        foreach (var (id, bodyPart) in _body.GetBodyChildren(body))
        {
            var target = _body.GetTargetBodyPart(bodyPart);
            if (target == null)
                continue;

            if (!TryComp<WoundableComponent>(id, out var woundable)
                || !TryComp<DamageableComponent>(id, out var damageable))
                continue;

            var nearestSeverity = WoundableSeverity.Loss;
            var damage = damageable.TotalDamage;

            foreach (var (severity, threshold) in woundable.Thresholds.OrderByDescending(kv => kv.Value))
            {
                if (damage <= 0)
                {
                    nearestSeverity = WoundableSeverity.Healthy;
                    break;
                }

                if (damage >= woundable.IntegrityCap)
                {
                    nearestSeverity = WoundableSeverity.Critical;
                    break;
                }

                if (damage > woundable.IntegrityCap - threshold)
                    continue;

                nearestSeverity = severity;
                break;
            }

            result[target.Value] = nearestSeverity;
        }

        return result;
    }

    public Dictionary<TargetBodyPart, WoundableSeverity> GetWoundableStatesOnBodyPainFeels(EntityUid body)
    {
        var result = new Dictionary<TargetBodyPart, WoundableSeverity>();

        foreach (var part in SharedTargetingSystem.GetValidParts())
        {
            result[part] = WoundableSeverity.Loss;
        }

        foreach (var (id, bodyPart) in _body.GetBodyChildren(body))
        {
            var target = _body.GetTargetBodyPart(bodyPart);
            if (target == null)
                continue;

            if (!TryComp<WoundableComponent>(id, out var woundable) || !TryComp<NerveComponent>(id, out var nerve))
                continue;

            var damageFeeling = woundable.WoundableIntegrity * nerve.PainFeels;

            var nearestSeverity = woundable.WoundableSeverity;
            foreach (var (severity, value) in woundable.Thresholds.OrderByDescending(kv => kv.Value))
            {
                if (damageFeeling <= 0)
                {
                    nearestSeverity = WoundableSeverity.Loss;
                    break;
                }

                if (damageFeeling >= woundable.IntegrityCap)
                {
                    nearestSeverity = WoundableSeverity.Healthy;
                    break;
                }

                if (damageFeeling < value)
                    continue;

                nearestSeverity = severity;
                break;
            }

            result[target.Value] = nearestSeverity;
        }

        return result;
    }

    /// <summary>
    /// Check if this woundable is root
    /// </summary>
    /// <param name="woundableEntity">Owner of the woundable</param>
    /// <param name="woundable">woundable component</param>
    /// <returns>true if the woundable is the root of the hierarchy</returns>
    public bool IsWoundableRoot(EntityUid woundableEntity, WoundableComponent? woundable = null)
    {
        return Resolve(woundableEntity, ref woundable, false)
            && woundable.RootWoundable == woundableEntity;
    }

    /// <summary>
    /// Retrieves all wounds associated with a specified entity.
    /// </summary>
    /// <param name="targetEntity">The UID of the target entity.</param>
    /// <param name="targetWoundable">Optional: The WoundableComponent of the target entity.</param>
    /// <returns>An enumerable collection of tuples containing EntityUid and WoundComponent pairs.</returns>
    public IEnumerable<Entity<WoundComponent>> GetAllWounds(EntityUid targetEntity,
        WoundableComponent? targetWoundable = null)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false))
            yield break;

        foreach (var (_, childWoundable) in GetAllWoundableChildren(targetEntity, targetWoundable))
        {
            if (childWoundable.Wounds == null)
                continue;

            foreach (var woundEntity in childWoundable.Wounds.ContainedEntities)
                yield return (woundEntity, Comp<WoundComponent>(woundEntity));

        }
    }

    /// <summary>
    /// Gets all woundable children of a specified woundable
    /// </summary>
    /// <param name="targetEntity">Owner of the woundable</param>
    /// <param name="targetWoundable"></param>
    /// <returns>Enumerable to the found children</returns>
    public IEnumerable<Entity<WoundableComponent>> GetAllWoundableChildren(EntityUid targetEntity,
        WoundableComponent? targetWoundable = null)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false))
            yield break;

        foreach (var childEntity in targetWoundable.ChildWoundables)
        {
            if (!TryComp(childEntity, out WoundableComponent? childWoundable))
                continue;
            foreach (var value in GetAllWoundableChildren(childEntity, childWoundable))
            {
                yield return value;
            }
        }

        yield return (targetEntity, targetWoundable);
    }

    /// <summary>
    /// Parents a woundable to another
    /// </summary>
    /// <param name="parentEntity">Owner of the new parent</param>
    /// <param name="childEntity">Owner of the woundable we want to attach</param>
    /// <param name="parentWoundable">The new parent woundable component</param>
    /// <param name="childWoundable">The woundable we are attaching</param>
    /// <returns>true if successful</returns>
    public bool AddWoundableToParent(
        EntityUid parentEntity,
        EntityUid childEntity,
        WoundableComponent? parentWoundable = null,
        WoundableComponent? childWoundable = null)
    {
        if (!Resolve(parentEntity, ref parentWoundable, false)
            || !Resolve(childEntity, ref childWoundable, false)
            || childWoundable.ParentWoundable == null)
            return false;

        InternalAddWoundableToParent(parentEntity, childEntity, parentWoundable, childWoundable);
        return true;
    }

    /// <summary>
    /// Removes a woundable from its parent (if present)
    /// </summary>
    /// <param name="parentEntity">Owner of the parent woundable</param>
    /// <param name="childEntity">Owner of the child woundable</param>
    /// <param name="parentWoundable"></param>
    /// <param name="childWoundable"></param>
    /// <returns>true if successful</returns>
    public bool RemoveWoundableFromParent(
        EntityUid parentEntity,
        EntityUid childEntity,
        WoundableComponent? parentWoundable = null,
        WoundableComponent? childWoundable = null)
    {
        if (!Resolve(parentEntity, ref parentWoundable, false)
            || !Resolve(childEntity, ref childWoundable, false)
            || childWoundable.ParentWoundable == null)
            return false;

        InternalRemoveWoundableFromParent(parentEntity, childEntity, parentWoundable, childWoundable);
        return true;
    }


    /// <summary>
    /// Finds all children of a specified woundable that have a specific component
    /// </summary>
    /// <param name="targetEntity"></param>
    /// <param name="targetWoundable"></param>
    /// <typeparam name="T">the type of the component we want to find</typeparam>
    /// <returns>Enumerable to the found children</returns>
    public IEnumerable<Entity<WoundableComponent, T>> GetAllWoundableChildrenWithComp<T>(EntityUid targetEntity,
        WoundableComponent? targetWoundable = null) where T: Component, new()
    {
        if (!Resolve(targetEntity, ref targetWoundable, false))
            yield break;

        foreach (var childEntity in targetWoundable.ChildWoundables)
        {
            if (!TryComp(childEntity, out WoundableComponent? childWoundable))
                continue;

            foreach (var value in GetAllWoundableChildrenWithComp<T>(childEntity, childWoundable))
            {
                yield return value;
            }
        }

        if (!TryComp(targetEntity, out T? foundComp))
            yield break;

        yield return (targetEntity, targetWoundable, foundComp);
    }

    /// <summary>
    /// Get the wounds present on a specific woundable
    /// </summary>
    /// <param name="targetEntity">Entity that owns the woundable</param>
    /// <param name="targetWoundable">Woundable component</param>
    /// <returns>An enumerable pointing to one of the found wounds</returns>
    public IEnumerable<Entity<WoundComponent>> GetWoundableWounds(EntityUid targetEntity,
        WoundableComponent? targetWoundable = null)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false)
            || targetWoundable.Wounds == null
            || targetWoundable.Wounds.Count == 0)
            yield break;

        foreach (var woundEntity in targetWoundable.Wounds.ContainedEntities.ToList())
        {
            yield return (woundEntity, Comp<WoundComponent>(woundEntity));
        }
    }

    /// <summary>
    /// Get the wounds present on a specific woundable, with a component you want
    /// </summary>
    /// <param name="targetEntity">Entity that owns the woundable</param>
    /// <param name="targetWoundable">Woundable component</param>
    /// <returns>An enumerable pointing to one of the found wounds, with the said component</returns>
    public IEnumerable<Entity<WoundComponent, T>> GetWoundableWoundsWithComp<T>(
        EntityUid targetEntity,
        WoundableComponent? targetWoundable = null) where T : Component, new()
    {
        if (!Resolve(targetEntity, ref targetWoundable, false)
            || targetWoundable.Wounds == null || targetWoundable.Wounds.Count == 0)
            yield break;

        foreach (var woundEntity in GetWoundableWounds(targetEntity, targetWoundable))
        {
            if (!TryComp<T>(woundEntity, out var foundComponent))
                continue;

            yield return (woundEntity, woundEntity, foundComponent);
        }
    }

    /// <summary>
    /// Checks for wounds on an entity that have exceeded their MangleSeverity threshold
    /// </summary>
    public bool HasWoundsExceedingMangleSeverity(EntityUid targetEntity, WoundableComponent? targetWoundable = null)
    {
        if (!Resolve(targetEntity, ref targetWoundable))
            return false;

        return GetWoundableWounds(targetEntity, targetWoundable)
            .Any(wound =>
                wound.Comp.MangleSeverity != null &&
                wound.Comp.WoundSeverity >= wound.Comp.MangleSeverity);
    }


    /// <summary>
    /// Returns you the sum of all wounds on this woundable
    /// </summary>
    /// <param name="targetEntity">The woundable uid</param>
    /// <param name="targetWoundable">The component</param>
    /// <param name="damageGroup">The damage group of said wounds</param>
    /// <param name="healable">Are the wounds supposed to be healable</param>
    /// <returns>The severity sum</returns>
    public FixedPoint2 GetWoundableSeverityPoint(
        EntityUid targetEntity,
        WoundableComponent? targetWoundable = null,
        string? damageGroup = null,
        bool healable = false,
        bool ignoreBlockers = false)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false)
            || targetWoundable.Wounds == null
            || targetWoundable.Wounds.Count == 0)
            return FixedPoint2.Zero;

        if (healable)
        {
            return GetWoundableWounds(targetEntity, targetWoundable)
                .Where(wound => wound.Comp.DamageGroup?.ID == damageGroup || damageGroup == null)
                .Where(wound => CanHealWound(wound, wound.Comp, ignoreBlockers))
                .Aggregate(FixedPoint2.Zero, (current, wound) => current + wound.Comp.WoundSeverityPoint);
        }

        return GetWoundableWounds(targetEntity, targetWoundable)
            .Where(wound => wound.Comp.DamageGroup?.ID == damageGroup || damageGroup == null)
            .Aggregate(FixedPoint2.Zero, (current, wound) => current + wound.Comp.WoundSeverityPoint);
    }

    /// <summary>
    /// Returns you the integrity damage the woundable has
    /// </summary>
    /// <param name="targetEntity">The woundable uid</param>
    /// <param name="targetWoundable">The component</param>
    /// <param name="damageGroup">The damage group of wounds that induced the damage</param>
    /// <param name="healable">Is the integrity damage healable</param>
    /// <returns>The integrity damage</returns>
    public FixedPoint2 GetWoundableIntegrityDamage(
        EntityUid targetEntity,
        WoundableComponent? targetWoundable = null,
        string? damageGroup = null,
        bool healable = false,
        bool ignoreBlockers = false)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false)
            || targetWoundable.Wounds == null
            || targetWoundable.Wounds.Count == 0)
            return FixedPoint2.Zero;

        if (healable)
        {
            return GetWoundableWounds(targetEntity, targetWoundable)
                .Where(wound => wound.Comp.DamageGroup?.ID == damageGroup || damageGroup == null)
                .Where(wound => CanHealWound(wound, wound.Comp, ignoreBlockers))
                .Aggregate(FixedPoint2.Zero, (current, wound) => current + wound.Comp.WoundIntegrityDamage);
        }

        return GetWoundableWounds(targetEntity, targetWoundable)
            .Where(wound => wound.Comp.DamageGroup?.ID == damageGroup || damageGroup == null)
            .Aggregate(FixedPoint2.Zero, (current, wound) => current + wound.Comp.WoundIntegrityDamage);
    }

    #endregion
}
