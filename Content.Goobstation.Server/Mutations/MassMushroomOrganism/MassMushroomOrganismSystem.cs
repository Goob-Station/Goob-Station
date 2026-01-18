using Content.Goobstation.Shared.Hydroponics.Mutations.MassMushroomOrganism;
using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Server.Chat;
using Content.Server.Chat.Systems;
using Content.Server.Drunk;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions.Components;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Drugs;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Mutations.MassMushroomOrganism;

public sealed class MassMushroomOrganismSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly HungerSystem _hungerSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly DrunkSystem _drunkSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly AutoEmoteSystem _emote = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    [ValidatePrototypeId<DamageTypePrototype>]
    private const string PoisonDamageType = "Poison";
    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string FactionType = "SimpleHostile";
    [ValidatePrototypeId<AutoEmotePrototype>]
    private const string EmoteCoughPrototypeId = "Cough";
    [ValidatePrototypeId<AutoEmotePrototype>]
    private const string EmoteLaughPrototypeId = "Laugh";
    [ValidatePrototypeId<SpeciesPrototype>]
    private const string MassMushroomOrganismSpecies = "MassMushroomOrganism";
    public override void Initialize()
    {
        SubscribeLocalEvent<FungalInfectionComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<MassMushroomOrganismComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<MassMushroomOrganismComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<MassMushroomOrganismHostComponent, FungalGrowthEvent>(OnFungalGrowth);
        SubscribeLocalEvent<MassMushroomOrganismHostComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<SporeDivisionComponent, ComponentInit>(OnSporeDivisionInit);
        SubscribeLocalEvent<SporeDivisionComponent, SporeDivisionEvent>(OnSporeDivisionEvent);
        SubscribeLocalEvent<SporeDivisionComponent, SporeDivisionDoAfterEvent>(OnSporeDivisionDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var cur = _timing.CurTime;

        var query = EntityQueryEnumerator<FungalInfectionComponent>();
        while (query.MoveNext(out var uid, out var infection))
        {
            if (cur < infection.NextUpdateTime || infection.CurrentPhase == FungalInfectionPhase.None)
                continue;

            infection.NextUpdateTime = cur + infection.UpdateInterval;
            infection.InfectionProgress += infection.InfectionProgressBonus;

            UpdateCurrentThreshold(infection);
            ApplyThresholdEffects(uid, infection);
        }
    }

    private void OnMapInit(Entity<FungalInfectionComponent> ent, ref MapInitEvent args)
    {
        var (_, comp) = ent;

        var phasesCount = comp.InfectionPhases.Count;
        if (comp.InfectionThresholds.Count <= 0 || phasesCount <= 0)
            return;

        comp.UpdateInterval = comp.TimeToNewPhase * comp.InfectionProgressBonus / 100.0f;
    }

    private void OnEquipped(EntityUid uid, MassMushroomOrganismComponent component, GotEquippedEvent args)
    {
        var host = EnsureComp<MassMushroomOrganismHostComponent>(args.Equipee);
        if (TryComp<HumanoidAppearanceComponent>(args.Equipee, out var humanoid) &&
            humanoid.Species == MassMushroomOrganismSpecies)
            return;

        var action = _actions.AddAction(args.Equipee, component.Action);
        host.AttachedMushroomOrganism = uid;
        host.ActionFungalGrowth = action;

        if (TryComp<ActionComponent>(action, out var actionComp) &&
            _mobState.IsDead(args.Equipee))
            _actions.PerformAction(args.Equipee, (action.Value, actionComp));
    }

    private void OnUnequipped(EntityUid uid, MassMushroomOrganismComponent component, GotUnequippedEvent args)
    {
        if (!TryComp<MassMushroomOrganismHostComponent>(args.Equipee, out var host))
            return;

        _actions.RemoveAction(host.ActionFungalGrowth);
        RemComp<MassMushroomOrganismHostComponent>(args.Equipee);
    }

    private void OnFungalGrowth(EntityUid uid, MassMushroomOrganismHostComponent component, FungalGrowthEvent args)
    {
        EnsureComp<UnremoveableComponent>(component.AttachedMushroomOrganism);

        if (args.Handled)
            return;
        args.Handled = true;

        var fungal = EnsureComp<FungalInfectionComponent>(uid);
        fungal.NextUpdateTime += _timing.CurTime + fungal.UpdateInterval;
        fungal.CurrentPhase = FungalInfectionPhase.First;
        fungal.InfectionProgress = 0f;
        fungal.LastThreshold = FungalInfectionThreshold.None;
        fungal.CurrentThreshold = FungalInfectionThreshold.None;

        _actions.RemoveAction(args.Action.Owner);
        _popup.PopupEntity(Loc.GetString("mass-mushroom-organism-fungal-growth-action"), uid, uid);
        _appearance.SetData(uid, FungalInfectionPhaseVisualsStage.InfectionStage, FungalInfectionPhase.First);
    }
    private string GetThresholdLocKey(FungalInfectionPhase phase, FungalInfectionThreshold threshold)
    {
        if (phase is not (FungalInfectionPhase.First or FungalInfectionPhase.Second) ||
            threshold is FungalInfectionThreshold.None)
            return string.Empty;

        var phaseNum = phase == FungalInfectionPhase.First ? 1 : 2;
        var thrNum = (int) threshold;

        return $"mass-mushroom-organism-phase-{phaseNum}-threshold-{thrNum}";
    }
    private void UpdateCurrentThreshold(FungalInfectionComponent comp)
    {
        var newThreshold = FungalInfectionThreshold.None;
        var highestValue = 0f;

        foreach (var pair in comp.InfectionThresholds)
        {
            if (comp.InfectionProgress >= pair.Value && pair.Value > highestValue)
            {
                highestValue = pair.Value;
                newThreshold = pair.Key;
            }
        }

        comp.CurrentThreshold = newThreshold;
    }

    private void ApplyThresholdEffects(EntityUid uid, FungalInfectionComponent comp)
    {
        var threshold = comp.CurrentThreshold;
        var thresholdChanged = threshold != comp.LastThreshold;

        if (thresholdChanged)
        {
            var key = GetThresholdLocKey(comp.CurrentPhase, threshold);
            if (!string.IsNullOrEmpty(key))
                _popup.PopupEntity(Loc.GetString(key), uid, uid);

            comp.LastThreshold = threshold;
        }

        switch (threshold)
        {
            case FungalInfectionThreshold.First:
                comp.IsHungerThirstModifyInitialized = true;

                if (comp.CurrentPhase == FungalInfectionPhase.Second)
                {
                    _drunkSystem.TryApplyDrunkenness(uid, comp.DrunkennessValue, false);
                    _emote.AddEmote(uid, EmoteLaughPrototypeId);
                }
                break;

            case FungalInfectionThreshold.Second:
                _movementSpeedModifier.ChangeBaseSpeed(uid, comp.WalkSpeedValueFirstPhase, comp.SprintSpeedValueFirstPhase, 20f);

                if (comp.CurrentPhase == FungalInfectionPhase.Second)
                    EnsureComp<TemporaryBlindnessComponent>(uid);
                break;

            case FungalInfectionThreshold.Third:
                comp.IsPoisonDamageInitialized = true;
                EnsureComp<AutoEmoteComponent>(uid);
                _emote.AddEmote(uid, EmoteCoughPrototypeId);

                if (comp.CurrentPhase == FungalInfectionPhase.Second)
                {
                    comp.IsPoisonDamageMultiplyed = true;
                    _movementSpeedModifier.ChangeBaseSpeed(uid, comp.WalkSpeedValueSecondPhase, comp.SprintSpeedValueSecondPhase, 20f);
                    EnsureComp<SeeingRainbowsStatusEffectComponent>(uid);
                }
                break;

            case FungalInfectionThreshold.Fourth:
                if (comp.CurrentPhase == FungalInfectionPhase.Second)
                    MushromifyTarget(uid, comp);
                break;
        }

        if (comp.IsHungerThirstModifyInitialized)
        {
            if (!TryComp<HungerComponent>(uid, out var hunger) || !TryComp<ThirstComponent>(uid, out var thirst))
                return;

            _hungerSystem.ModifyHunger(uid, -comp.HungerModifyValue, hunger);
            _thirst.ModifyThirst(uid, thirst, -comp.ThirstModifyValue);
        }

        if (comp.IsPoisonDamageInitialized)
        {
            var damageValue = comp.IsPoisonDamageMultiplyed
                ? comp.PoisonDamage * comp.PoisonDamageMultiply
                : comp.PoisonDamage;
            var poisonDamage = new DamageSpecifier(_prototypes.Index<DamageTypePrototype>(PoisonDamageType), damageValue);
            _damageable.TryChangeDamage(uid, poisonDamage, targetPart: TargetBodyPart.All);
        }

        if (comp.InfectionProgress <= 100f)
            return;

        var next = GetNextPhase(comp.CurrentPhase);
        if (next != null)
        {
            comp.CurrentPhase = next.Value;
            _appearance.SetData(uid, FungalInfectionPhaseVisualsStage.InfectionStage, comp.CurrentPhase);
        }
        comp.InfectionProgress = 0f;
    }
    private FungalInfectionPhase? GetNextPhase(FungalInfectionPhase current)
    {
        return current == FungalInfectionPhase.Second ? null : current + 1;
    }

    private void MushromifyTarget(EntityUid target, FungalInfectionComponent comp)
    {
        var ent = _random.Pick(comp.MushroomPrototypes).Key;

        if (_polymorph.PolymorphEntity(target, ent) is not { } mushroom)
            return;
        _rejuvenate.PerformRejuvenate(mushroom);

        var faction = EnsureComp<NpcFactionMemberComponent>(mushroom);
        _npcFaction.AddFaction((mushroom, faction), FactionType);

        if (!_mind.TryGetMind(mushroom, out _, out _))
        {
            var ghostRole = EnsureComp<GhostRoleComponent>(mushroom);
            EnsureComp<GhostTakeoverAvailableComponent>(mushroom);
            ghostRole.RoleName = Loc.GetString("massmushroomorganism-role-name");
            ghostRole.RoleDescription = Loc.GetString("massmushroomorganism-role-desc");
        }
    }

    private void OnMobStateChanged(EntityUid uid,
        MassMushroomOrganismHostComponent component,
        MobStateChangedEvent args)
    {
        var action = component.ActionFungalGrowth;
        if (args.NewMobState != MobState.Dead ||
            action == null ||
            !TryComp<ActionComponent>(action, out var actionComp))
            return;

        _actions.PerformAction(uid, (action.Value, actionComp));
    }

    private void OnSporeDivisionInit(EntityUid uid, SporeDivisionComponent comp, ComponentInit args)
    {
        _actions.AddAction(uid, comp.Action);
    }

    private void OnSporeDivisionEvent(EntityUid uid, SporeDivisionComponent comp, SporeDivisionEvent args)
    {
        if (TryComp<HungerComponent>(uid, out var hungerComp)
            && _hungerSystem.IsHungerBelowState(uid,
                comp.MinHungerThreshold,
                _hungerSystem.GetHunger(hungerComp) - comp.HungerCost,
                hungerComp))
        {
            _popup.PopupClient(Loc.GetString("spore-division-failure-hunger"), uid);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, uid, comp.DoAfterLength, new SporeDivisionDoAfterEvent(), uid)
        {
            BlockDuplicate = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            CancelDuplicate = true,
            MultiplyDelay = false,
        };

        _doAfterSystem.TryStartDoAfter(doAfter);
    }

    private void OnSporeDivisionDoAfter(Entity<SporeDivisionComponent> ent, ref SporeDivisionDoAfterEvent args)
    {
        var (uid, comp) = ent;

        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        var activeItem = _hands.GetActiveItem(uid);
        if (activeItem != null &&
            _solution.TryGetFitsInDispenser(activeItem.Value, out var soln, out _))
        {
            var reagent = new Solution();
            reagent.AddReagent(comp.Reagent, comp.ReagentAmount);

            _solution.TryAddSolution(soln.Value, reagent);
        }
        else
        {
            var coordinates = Transform(uid).Coordinates;

            var puddleSolution = new Solution();
            puddleSolution.AddReagent(comp.Reagent, comp.ReagentAmount);

            _puddle.TrySpillAt(coordinates, puddleSolution, out _);
        }

        _hungerSystem.ModifyHunger(uid, -comp.HungerCost);

        args.Repeat = true;
    }
}
