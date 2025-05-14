// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.CCVar;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

public sealed partial class WoundSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;

    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    [Dependency] private readonly DamageableSystem _damageable = default!;

    // I'm the one.... who throws........
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;

    private float _medicalHealingTickrate = 2f;
    private Queue<Entity<WoundableComponent?>> _healQueue = new(); // evil stateful data in system

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WoundComponent, ComponentGetState>(OnWoundComponentGet);
        SubscribeLocalEvent<WoundComponent, ComponentHandleState>(OnWoundComponentHandleState);
        SubscribeLocalEvent<WoundableComponent, ComponentGetState>(OnWoundableComponentGet);
        SubscribeLocalEvent<WoundableComponent, ComponentHandleState>(OnWoundableComponentHandleState);
        InitWounding();
        Subs.CVar(_cfg, SurgeryCVars.MedicalHealingTickrate, val => _medicalHealingTickrate = val, true);
    }

    private void OnWoundComponentGet(EntityUid uid, WoundComponent comp, ref ComponentGetState args)
    {
        var state = new WoundComponentState
        {
            HoldingWoundable =
                TryGetNetEntity(comp.HoldingWoundable, out var holdingWoundable)
                    ? holdingWoundable.Value
                    : NetEntity.Invalid,

            WoundSeverityPoint = comp.WoundSeverityPoint,
            WoundableIntegrityMultiplier = comp.WoundableIntegrityMultiplier,

            WoundType = comp.WoundType,

            DamageGroup = comp.DamageGroup,
            DamageType = comp.DamageType,

            ScarWound = comp.ScarWound,
            IsScar = comp.IsScar,

            WoundSeverity = comp.WoundSeverity,

            WoundVisibility = comp.WoundVisibility,

            CanBeHealed = comp.CanBeHealed,
        };

        args.State = state;
    }

    private void OnWoundComponentHandleState(EntityUid uid, WoundComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not WoundComponentState state)
            return;

        // Predict events on client!!
        var holdingWoundable = TryGetEntity(state.HoldingWoundable, out var e) ? e.Value : EntityUid.Invalid;
        if (holdingWoundable != component.HoldingWoundable)
        {
            if (holdingWoundable == EntityUid.Invalid)
            {
                if (TryComp(holdingWoundable, out WoundableComponent? oldParentWoundable) &&
                    TryComp(oldParentWoundable.RootWoundable, out WoundableComponent? oldWoundableRoot))
                {
                    var ev2 = new WoundRemovedEvent(component, oldParentWoundable, oldWoundableRoot);
                    RaiseLocalEvent(component.HoldingWoundable, ref ev2);
                }
            }
            else
            {
                if (TryComp(holdingWoundable, out WoundableComponent? parentWoundable) &&
                    TryComp(parentWoundable.RootWoundable, out WoundableComponent? woundableRoot))
                {
                    var ev = new WoundAddedEvent(component, parentWoundable, woundableRoot);
                    RaiseLocalEvent(uid, ref ev);

                    var ev1 = new WoundAddedEvent(component, parentWoundable, woundableRoot);
                    RaiseLocalEvent(holdingWoundable, ref ev1);

                    var bodyPart = Comp<BodyPartComponent>(holdingWoundable);
                    if (bodyPart.Body.HasValue)
                    {
                        var ev2 = new WoundAddedOnBodyEvent((uid, component), parentWoundable, woundableRoot);
                        RaiseLocalEvent(bodyPart.Body.Value, ref ev2);
                    }
                }
            }
        }

        component.HoldingWoundable = holdingWoundable;

        if (component.WoundSeverityPoint != state.WoundSeverityPoint)
        {
            var ev = new WoundSeverityPointChangedEvent(component,
                component.WoundSeverityPoint,
                state.WoundSeverityPoint);
            RaiseLocalEvent(uid, ref ev);

            // TODO: On body changed events aren't predicted, welp
        }

        component.WoundSeverityPoint = state.WoundSeverityPoint;
        component.WoundableIntegrityMultiplier = state.WoundableIntegrityMultiplier;

        if (component.HoldingWoundable != EntityUid.Invalid)
        {
            UpdateWoundableIntegrity(component.HoldingWoundable);
            CheckWoundableSeverityThresholds(component.HoldingWoundable);
        }

        component.WoundType = state.WoundType;

        component.DamageGroup = state.DamageGroup;
        if (state.DamageType != null)
            component.DamageType = state.DamageType;

        component.ScarWound = state.ScarWound;
        component.IsScar = state.IsScar;

        if (component.WoundSeverity != state.WoundSeverity)
        {
            var ev = new WoundSeverityChangedEvent(component.WoundSeverity, state.WoundSeverity);
            RaiseLocalEvent(uid, ref ev);
        }

        component.WoundSeverity = state.WoundSeverity;
        component.WoundVisibility = state.WoundVisibility;
        component.CanBeHealed = state.CanBeHealed;
    }

    private void OnWoundableComponentGet(EntityUid uid, WoundableComponent comp, ref ComponentGetState args)
    {
        var state = new WoundableComponentState
        {
            ParentWoundable = TryGetNetEntity(comp.ParentWoundable, out var parentWoundable) ? parentWoundable : null,
            RootWoundable = TryGetNetEntity(comp.RootWoundable, out var rootWoundable)
                ? rootWoundable.Value
                : NetEntity.Invalid,

            ChildWoundables =
                comp.ChildWoundables
                    .Select(woundable => TryGetNetEntity(woundable, out var ne)
                        ? ne.Value
                        : NetEntity.Invalid)
                    .ToHashSet(),
            // Attached and Detached -Woundable events are handled on client with containers

            AllowWounds = comp.AllowWounds,

            DamageContainerID = comp.DamageContainerID,

            DodgeChance = comp.DodgeChance,

            WoundableIntegrity = comp.WoundableIntegrity,
            HealAbility = comp.HealAbility,

            SeverityMultipliers =
                comp.SeverityMultipliers
                    .Select(multiplier
                        => (TryGetNetEntity(multiplier.Key, out var ne) ? ne.Value : NetEntity.Invalid,
                            multiplier.Value))
                    .ToDictionary(),
            HealingMultipliers =
                comp.HealingMultipliers
                    .Select(multiplier
                        => (TryGetNetEntity(multiplier.Key, out var ne) ? ne.Value : NetEntity.Invalid,
                            multiplier.Value))
                    .ToDictionary(),

            WoundableSeverity = comp.WoundableSeverity,
        };

        args.State = state;
    }

    private void OnWoundableComponentHandleState(EntityUid uid, WoundableComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not WoundableComponentState state)
            return;

        TryGetEntity(state.ParentWoundable, out component.ParentWoundable);
        TryGetEntity(state.RootWoundable, out var rootWoundable);
        component.RootWoundable = rootWoundable ?? EntityUid.Invalid;

        component.ChildWoundables = state.ChildWoundables
            .Select(x => TryGetEntity(x, out var y) ? y.Value : EntityUid.Invalid)
            .Where(x => x.Valid)
            .ToHashSet();
        // Attached and Detached -Woundable events are handled on client with containers

        component.AllowWounds = state.AllowWounds;

        component.DamageContainerID = state.DamageContainerID;

        component.DodgeChance = state.DodgeChance;
        component.HealAbility = state.HealAbility;

        component.SeverityMultipliers =
            state.SeverityMultipliers
                .Select(multiplier
                    => (TryGetEntity(multiplier.Key, out var ne) ? ne.Value : EntityUid.Invalid, multiplier.Value))
                .ToDictionary();
        component.HealingMultipliers =
            state.HealingMultipliers
                .Select(multiplier
                    => (TryGetEntity(multiplier.Key, out var ne) ? ne.Value : EntityUid.Invalid, multiplier.Value))
                .ToDictionary();

        if (component.WoundableIntegrity != state.WoundableIntegrity)
        {
            var bodyPart = Comp<BodyPartComponent>(uid);

            var ev = new WoundableIntegrityChangedEvent(component.WoundableIntegrity, state.WoundableIntegrity);
            RaiseLocalEvent(uid, ref ev);

            var bodySeverity = FixedPoint2.Zero;
            if (bodyPart.Body.HasValue)
            {
                var rootPart = Comp<BodyComponent>(bodyPart.Body.Value).RootContainer.ContainedEntity;
                if (rootPart.HasValue)
                {
                    foreach (var woundable in GetAllWoundableChildren(rootPart.Value))
                    {
                        if (!MetaData(woundable).Initialized)
                            continue;

                        // The first check is for the root (chest) part entities, the other one is for attached entities
                        if (woundable.Comp.RootWoundable == woundable.Owner && woundable.Owner != rootPart)
                            continue;

                        bodySeverity += GetWoundableIntegrityDamage(woundable, woundable);
                    }
                }

                var ev1 = new WoundableIntegrityChangedOnBodyEvent(
                    (uid, component),
                    bodySeverity - (component.WoundableIntegrity - state.WoundableIntegrity),
                    bodySeverity);
                RaiseLocalEvent(bodyPart.Body.Value, ref ev1);
            }
        }

        component.WoundableIntegrity = state.WoundableIntegrity;

        if (component.WoundableSeverity != state.WoundableSeverity)
        {
            var ev = new WoundableSeverityChangedEvent(component.WoundableSeverity, state.WoundableSeverity);
            RaiseLocalEvent(uid, ref ev);
        }

        component.WoundableSeverity = state.WoundableSeverity;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted || _healQueue.Count == 0)
            return;

        var beginEnt = _healQueue.Peek();
        do
        {
            var ent = _healQueue.Peek();
            BodyComponent? body = null;
            // remove it from queue and go on if it's not real
            if (TerminatingOrDeleted(ent) || !Resolve(ent, ref ent.Comp, ref body))
            {
                _healQueue.Dequeue();
                if (ent == beginEnt && _healQueue.Count != 0)
                    beginEnt = _healQueue.Peek();
                continue;
            }

            // queue is supposed to be sorted time-wise
            if (body.HealAt > _timing.CurTime)
                break;

            // it's real and it's time, move it to the end of the queue
            body.HealAt += TimeSpan.FromSeconds(1f / _medicalHealingTickrate);
            _healQueue.Dequeue();
            _healQueue.Enqueue(ent);

            if (Paused(ent) || !_body.TryGetRootPart(ent, out var rootPart, body: body))
                continue;

            foreach (var woundable in GetAllWoundableChildren(rootPart.Value))
                ProcessHealing(woundable, frameTime);

        } while (_healQueue.Count != 0 && _healQueue.Peek() != beginEnt);
    }

    private void ProcessHealing(Entity<WoundableComponent> woundable, float frameTime)
    {
        var bleedWounds = GetWoundableWoundsWithComp<BleedInflicterComponent>(woundable)
            .Where(wound => wound.Comp2.BleedingAmount > 0)
            .ToArray();

        var bleedingAmount = bleedWounds.Aggregate(FixedPoint2.Zero,
            (current, wound) => current + wound.Comp2.BleedingAmount);

        if (bleedingAmount > woundable.Comp.BleedsThreshold)
            return;

        if (bleedWounds.Length > 0)
        {
            var bleedTreatment = woundable.Comp.BleedingTreatmentAbility / bleedWounds.Length;
            TryHealBleedingWounds(woundable, (float) -bleedTreatment, out _, woundable);
        }

        var woundsToHeal = GetWoundableWounds(woundable).Where(wound => CanHealWound(wound, wound)).ToList();

        if (woundsToHeal.Count == 0)
            return;

        var healAmount = -woundable.Comp.HealAbility / woundsToHeal.Count;

        foreach (var x in woundsToHeal)
        {
            ApplyWoundSeverity(x,
                ApplyHealingRateMultipliers(x, woundable, healAmount),
                x);
        }
    }
}
