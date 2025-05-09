using Content.Shared._Shitmed.CCVar;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
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
using Robust.Shared.Threading;
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
    [Dependency] private readonly IParallelManager _parallel = default!;

    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
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
                TryGetNetEntity(comp.HoldingWoundable, out var holdingWoundable) ? holdingWoundable.Value : NetEntity.Invalid,

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
            var ev = new WoundSeverityPointChangedEvent(component, component.WoundSeverityPoint, state.WoundSeverityPoint);
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
            RootWoundable = TryGetNetEntity(comp.RootWoundable, out var rootWoundable) ? rootWoundable.Value : NetEntity.Invalid,

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
                        => (TryGetNetEntity(multiplier.Key, out var ne) ? ne.Value : NetEntity.Invalid, multiplier.Value))
                    .ToDictionary(),
            HealingMultipliers =
                comp.HealingMultipliers
                    .Select(multiplier
                        => (TryGetNetEntity(multiplier.Key, out var ne) ? ne.Value : NetEntity.Invalid, multiplier.Value))
                    .ToDictionary(),

            WoundableSeverity = comp.WoundableSeverity,
            HealingRateAccumulated = comp.HealingRateAccumulated,
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

        component.HealingRateAccumulated = state.HealingRateAccumulated;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var timeToHeal = 1 / _medicalHealingTickrate;
        var query = EntityQueryEnumerator<WoundableComponent, MetaDataComponent>();
        while (query.MoveNext(out var ent, out var woundable, out var metaData))
        {
            if (Paused(ent, metaData))
                continue;

            woundable.HealingRateAccumulated += frameTime;
            if (woundable.HealingRateAccumulated < timeToHeal)
                continue;

            if (woundable.Wounds == null || woundable.Wounds.Count == 0)
                continue;

            woundable.HealingRateAccumulated -= timeToHeal;

            var bleedWounds = GetWoundableWoundsWithComp<BleedInflicterComponent>(ent, woundable)
                .Where(wound => wound.Comp2.BleedingAmount > 0)
                .ToArray();

            var bleedingAmount = bleedWounds.Aggregate(FixedPoint2.Zero,
                    (current, wound) => current + wound.Comp2.BleedingAmount);

            if (bleedingAmount > woundable.BleedsThreshold)
                continue;

            if (bleedWounds.Length > 0)
            {
                var bleedTreatment = woundable.BleedingTreatmentAbility / bleedWounds.Length;
                var result = TryHealBleedingWounds(ent, (float) -bleedTreatment, out var modifiedBleed, woundable);
            }

            var woundsToHeal =
                GetWoundableWounds(ent, woundable).Where(wound => CanHealWound(wound, wound)).ToList();

            if (woundsToHeal.Count == 0)
                continue;

            var healAmount = -woundable.HealAbility / woundsToHeal.Count;

            Entity<WoundableComponent> owner = (ent, woundable);
            foreach (var x in woundsToHeal)
            {
                ApplyWoundSeverity(x,
                    ApplyHealingRateMultipliers(x, owner, healAmount, owner, x),
                    x);
            }
        }
    }
}
