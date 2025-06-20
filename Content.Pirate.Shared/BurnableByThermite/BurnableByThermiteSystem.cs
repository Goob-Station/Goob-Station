using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Destructible;
using Content.Shared.IgnitionSource;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Pirate.Shared.BurnableByThermite;

public sealed partial class SharedBurnableByThermiteSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedDestructibleSystem _destructibleSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BurnableByThermiteComponent, InteractUsingEvent>(OnInteract);
    }
    public void OnInteract(EntityUid uid, BurnableByThermiteComponent component, InteractUsingEvent args)
    {
        if (TryComp<SolutionContainerManagerComponent>(args.Used, out var beakerSolutionContainerComponent))
            OnInteractWithBeaker(uid, component, args.Used, args, beakerSolutionContainerComponent);
        if (TryComp<IgnitionSourceComponent>(args.Used, out var ignitionSourceComponent))
            OnInteractWithLighter(new(uid, component), args, ignitionSourceComponent);
        // TryComp<IgnitionSourceComponent>(args.Used, out var ignitionSourceComponent);

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BurnableByThermiteComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.Ignited && !component.Burning) continue;
            if (component.Ignited) OnUpdateIgnited(component);
            if (component.Burning) OnUpdateBurning(uid, component, frameTime);
        }

    }
    public void OnUpdateBurning(EntityUid uid, BurnableByThermiteComponent component, float deltaT)
    {
        SetSpriteData(uid, BurnableByThermiteVisuals.OnFireFull, false);
        if (_gameTiming.CurTime.TotalSeconds - component.BurningStartTime >= component.BurnTime)
        {
            component.Burning = false;
            return;
        }
        component.TotalDamageDealt += component.DPS * deltaT;
        if (component.TotalDamageDealt >= component.TotalDamageUntilMelting)
        {
            component.Burning = false;
            _destructibleSystem.DestroyEntity(uid);
            return;
        }

    }
    public void OnUpdateIgnited(BurnableByThermiteComponent component)
    {
        if (_gameTiming.CurTime.TotalSeconds - component.IgnitionStartTime >= component.IgnitionTime)
        {
            component.Ignited = false;
            component.Burning = true;
            component.BurningStartTime = (float) _gameTiming.CurTime.TotalSeconds;
        }
    }
    public void OnInteractWithLighter(Entity<BurnableByThermiteComponent> ent, InteractUsingEvent args, IgnitionSourceComponent ignitionSourceComponent)
    {
        if (!ignitionSourceComponent.Ignited) return;
        if (ent.Comp.Ignited || ent.Comp.Burning) return;
        if (ent.Comp.ThermiteSolutionComponent is null) return;
        if (ent.Comp.ThermiteSolutionComponent.Solution.TryGetReagentQuantity(new("Thermite", null), out var thermiteQuantity) && thermiteQuantity == 0) return;
        SetSpriteData(ent.Owner, BurnableByThermiteVisuals.OnFireStart, true);
        _popupSystem.PopupClient(Loc.GetString("thermite-on-structure-ignited"), args.User, args.User, PopupType.MediumCaution);
        ent.Comp.Ignited = true;
        ent.Comp.IgnitionStartTime = (float) _gameTiming.CurTime.TotalSeconds;
        ent.Comp.ThermiteSolutionComponent.Solution.RemoveAllSolution();
    }

    public void OnInteractWithBeaker(EntityUid structure, BurnableByThermiteComponent component, EntityUid beaker, InteractUsingEvent args, SolutionContainerManagerComponent beakerSolutionContainerComponent)
    {
        if (beakerSolutionContainerComponent.Containers is null) return;
        if (beakerSolutionContainerComponent.Containers.Count == 0) return;

        foreach (var (_, solutionEntity) in _solutionSystem.EnumerateSolutions(beaker))
        {
            if (!solutionEntity.Comp.Solution.TryGetReagent(new ReagentId("Thermite", null), out var thermiteReagent))
                continue;
            EnsureComp<SolutionContainerManagerComponent>(structure, out var structureSolutionContainerComponent);
            _solutionSystem.EnsureSolution(structure, "thermite-solution", out var structureThermiteSolution, 10f);
            if (!_solutionSystem.TryGetSolution(new(structure, structureSolutionContainerComponent), "thermite-solution", out Entity<SolutionComponent>? structureThermiteSolutionEntity)) return;
            component.ThermiteSolutionComponent = structureThermiteSolutionEntity.Value; // Set the solution component to the one we just got.
            if (structureThermiteSolution is null) return;  // Just in case

            if (thermiteReagent.Quantity < structureThermiteSolution.MaxVolume)
            {
                _popupSystem.PopupClient(Loc.GetString("thermite-on-structure-not-enough"), args.User, args.User, PopupType.Small);
                continue;
            }
            if (thermiteReagent.Quantity >= structureThermiteSolution.MaxVolume)
            {
                TransferReagent(structureThermiteSolutionEntity.Value, thermiteReagent, solutionEntity, structure, args.User);
            }
        }

    }

    public void TransferReagent(Entity<SolutionComponent> to, ReagentQuantity reagent, Entity<SolutionComponent> from, EntityUid structure, EntityUid user)
    {
        _solutionSystem.TryAddReagent(to, reagent, out var transferedAmount);
        if (transferedAmount == 0)
        {
            _popupSystem.PopupClient(Loc.GetString("thermite-on-structure-full"), user, user, PopupType.Medium);
            return;
        }
        else
        {
            _popupSystem.PopupClient(Loc.GetString("thermite-on-structure-success"), user, user, PopupType.MediumCaution);
            _solutionSystem.RemoveReagent(from, "Thermite", transferedAmount);
            SetSpriteData(structure, BurnableByThermiteVisuals.CoveredInThermite, true);
            return;
        }
    }
    public void SetSpriteData(EntityUid uid, Enum visuals, bool state, bool disableOthers = true)
    {
        if (_appearanceSystem.TryGetData<bool>(uid, visuals, out var currentState))
        {
            if (currentState == state) return;
        }
        if (disableOthers)
        {
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.CoveredInThermite, false);
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.OnFireStart, false);
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.OnFireFull, false);
        }
        _appearanceSystem.SetData(uid, visuals, state);
    }
}
