// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.CCVar;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Weapons.Melee.Events;
using Content.Shared._Shitmed.Weapons.Ranged.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Gibbing.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

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
        SubscribeLocalEvent<WoundComponent, WoundSeverityPointChangedEvent>(OnWoundSeverityPointChanged);
        SubscribeLocalEvent<WoundableComponent, WoundHealAttemptOnWoundableEvent>(HealWoundsOnWoundableAttempt);
        SubscribeLocalEvent<WoundableComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<WoundableComponent, GetDoAfterDelayMultiplierEvent>(OnGetDoAfterDelayMultiplier);
        SubscribeLocalEvent<WoundableComponent, AttemptHandsMeleeEvent>(OnAttemptHandsMelee);
        SubscribeLocalEvent<WoundableComponent, AttemptHandsShootEvent>(OnAttemptHandsShoot);
        SubscribeLocalEvent<TraumaInflicterComponent, TraumaBeingRemovedEvent>(OnTraumaBeingRemoved);
    }

    #region Event Handling

    private void OnWoundableInit(EntityUid uid, WoundableComponent comp, ComponentInit args)
    {
        comp.RootWoundable = uid;
        comp.Wounds = _container.EnsureContainer<Container>(uid, WoundContainerId);
        comp.Bone = _container.EnsureContainer<Container>(uid, BoneContainerId);
        comp.SortedThresholds = comp.Thresholds.OrderByDescending(kv => kv.Value).ToArray();
    }

    private void OnWoundableMapInit(EntityUid uid, WoundableComponent comp, MapInitEvent args)
    {
        if (HasComp<BonelessComponent>(uid))
            return;

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

        if (TryComp<BodyPartComponent>(parentEntity, out var bodyPart)
            && bodyPart.Body is { } body)
            _trauma.UpdateBodyBoneAlert(body);
    }

    private void OnWoundableRemoved(EntityUid parentEntity, WoundableComponent parentWoundable, EntRemovedFromContainerMessage args)
    {
        if (!TryComp<WoundableComponent>(args.Entity, out var childWoundable)
            || !_net.IsServer)
            return;

        InternalRemoveWoundableFromParent(parentEntity, args.Entity, parentWoundable, childWoundable);

        if (TryComp<BodyPartComponent>(parentEntity, out var bodyPart)
            && bodyPart.Body is { } body)
            _trauma.UpdateBodyBoneAlert(body);
    }

    private void OnWoundableContentsGibAttempt(EntityUid uid, WoundableComponent comp, ref AttemptEntityContentsGibEvent args)
    {
        if (args.ExcludedContainers == null)
            args.ExcludedContainers = new List<string> { WoundContainerId, BoneContainerId };
        else
            args.ExcludedContainers.AddRange(new List<string> { WoundContainerId, BoneContainerId });
    }

    private void HealWoundsOnWoundableAttempt(Entity<WoundableComponent> woundable, ref WoundHealAttemptOnWoundableEvent args)
    {
        if (woundable.Comp.WoundableSeverity == WoundableSeverity.Severed)
            args.Cancelled = true;
    }

    private void OnWoundSeverityChanged(EntityUid wound, WoundComponent woundComponent, WoundSeverityChangedEvent args)
    {
        if (args.NewSeverity != WoundSeverity.Healed)
            return;

        RemoveWound(wound, woundComponent);
    }

    private void OnWoundSeverityPointChanged(EntityUid uid, WoundComponent component, WoundSeverityPointChangedEvent args)
    {
        if (TerminatingOrDeleted(uid)
            || TerminatingOrDeleted(component.HoldingWoundable)
            || !TryComp<WoundableComponent>(component.HoldingWoundable, out var woundable)
            || woundable.WoundableSeverity != WoundableSeverity.Mangled
            || !TryComp<BodyPartComponent>(component.HoldingWoundable, out var bodyPart))
            return;

        if (bodyPart.Body is not null)
            return;

        DestroyWoundable(component.HoldingWoundable, component.HoldingWoundable, woundable);
    }

    private void OnDamageChanged(EntityUid uid, WoundableComponent component, ref DamageChangedEvent args)
    {
        // Skip if there was no damage delta or if wounds aren't allowed
        if (args.UncappedDamage == null // Goobstation
            || !component.AllowWounds
            || !_net.IsServer)
            return;

        // Create or update wounds based on damage changes
        foreach (var (damageType, damageValue) in args.UncappedDamage.DamageDict)
        {
            if (damageValue == 0)
                continue; // Only create wounds for damage or healing

            if (TryComp<DamageableComponent>(uid, out var damageable)
                    && !damageable.Damage.DamageDict.ContainsKey(damageType))
                continue;

            if (damageValue < 0)
            {
                // Use HealWoundsCore directly to avoid redundant Update+Check per type
                HealWoundsCore(uid, -damageValue, damageType, out _, component, ignoreBlockers: args.IgnoreBlockers);
            }
            else
            {
                // Only create wound if it's a valid damage type for wounds
                if (!IsWoundPrototypeValid(damageType))
                    continue;

                TryInduceWound(uid,
                    damageType,
                    damageValue *
                    args.UncappedDamage.WoundSeverityMultipliers.GetValueOrDefault(damageType, 1),
                    out _,
                    component);
            }
        }

        // Update woundable integrity based on new damage, once for all types
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

    private void OnTraumaBeingRemoved(Entity<TraumaInflicterComponent> ent, ref TraumaBeingRemovedEvent args)
    {
        if (TryComp<WoundComponent>(ent, out var woundComp))
        {
            if (woundComp.WoundSeverity != WoundSeverity.Healed)
                return;
            RemoveWound(ent); // Remove wound method will perform the check on if there are any other wounds pending treatment
        }
    }

    #endregion

    public DamageGroupPrototype? GetDamageGroupByType(string id)
    {
        return _damageTypeToGroup.GetValueOrDefault(id);
    }
}
