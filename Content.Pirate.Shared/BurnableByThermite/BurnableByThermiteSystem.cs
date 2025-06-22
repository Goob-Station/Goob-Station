using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Destructible;
using Content.Shared.IgnitionSource;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Pirate.Shared.BurnableByThermite;

public sealed partial class SharedBurnableByThermiteSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedDestructibleSystem _destructibleSystem = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BurnableByThermiteComponent, InteractUsingEvent>(OnInteract);
    }

    // Interaction Handling
    public void OnInteract(EntityUid uid, BurnableByThermiteComponent component, InteractUsingEvent args)
    {
        if (TryComp<SolutionContainerManagerComponent>(args.Used, out var beakerSolutionContainerComponent))
            OnInteractWithBeaker(uid, component, args.Used, args, beakerSolutionContainerComponent);
        if (TryComp<IgnitionSourceComponent>(args.Used, out var ignitionSourceComponent))
            OnInteractWithLighter(new(uid, component), args, ignitionSourceComponent);
        // TryComp<IgnitionSourceComponent>(args.Used, out var ignitionSourceComponent);
    }
    public void OnInteractWithLighter(Entity<BurnableByThermiteComponent> ent, InteractUsingEvent args, IgnitionSourceComponent ignitionSourceComponent)
    {
        if (!ignitionSourceComponent.Ignited) return;
        if (ent.Comp.Ignited || ent.Comp.Burning) return;
        TryIgniteStructure(ent, args);
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

    // Update
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
        SetSpriteData(uid, BurnableByThermiteVisuals.OnFireFull, true);
        if (_gameTiming.CurTime.TotalSeconds - component.BurningStartTime >= component.BurnTime)
        {
            TryExtinguishStructure(new(uid, component));
            return;
        }
        component.TotalDamageDealt += component.DPS * deltaT;
        if (component.TotalDamageDealt >= component.TotalDamageUntilMelting)
        {
            OnEntityBurned(uid, component);
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

    // Secondary Media
    public void SetSpriteData(EntityUid uid, Enum visuals, bool state, bool disableOthers = false)
    {
        if (_appearanceSystem.TryGetData<bool>(uid, visuals, out var currentState))
            if (currentState == state) return;

        if (disableOthers)
        {
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.CoveredInThermite, false);
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.OnFireStart, false);
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.OnFireFull, false);
        }
        _appearanceSystem.SetData(uid, visuals, state);
    }
    public bool TryPlayBurningSound(EntityUid uid, BurnableByThermiteComponent component)
    {
        if (_audioSystem.IsPlaying(component.BurningSoundStream))
            _audioSystem.Stop(component.BurningSoundStream);
        var resolvedSound = _audioSystem.ResolveSound(component.BurningSound);
        component.BurningSoundStream = _audioSystem.PlayPvs(resolvedSound, uid)?.Entity;
        return true;
    }

    // Secondary Structure
    public void OnEntityBurned(EntityUid uid, BurnableByThermiteComponent component)
    {
        component.BurningSoundStream = _audioSystem.Stop(component.BurningSoundStream);
        _destructibleSystem.DestroyEntity(uid);
    }
    public bool TryExtinguishStructure(Entity<BurnableByThermiteComponent> ent)


    {
        if (!ent.Comp.Ignited && !ent.Comp.Burning) return false;
        SetSpriteData(ent.Owner, BurnableByThermiteVisuals.CoveredInThermite, false, true);
        ent.Comp.Ignited = false;
        ent.Comp.Burning = false;
        ent.Comp.BurningSoundStream = _audioSystem.Stop(ent.Comp.BurningSoundStream);
        if (ent.Comp.ThermiteSolutionComponent is null) return true;
        ent.Comp.ThermiteSolutionComponent.Solution.RemoveAllSolution();
        return true;
    }
    public bool TryIgniteStructure(Entity<BurnableByThermiteComponent> ent, InteractUsingEvent args)

    {
        if (ent.Comp.ThermiteSolutionComponent is null) return false;
        if (ent.Comp.ThermiteSolutionComponent.Solution.TryGetReagentQuantity(new("Thermite", null), out var thermiteQuantity) && thermiteQuantity == 0) return false;
        SetSpriteData(ent.Owner, BurnableByThermiteVisuals.OnFireStart, true);
        _pointLight.EnsureLight(ent.Owner);
        _pointLight.SetColor(ent.Owner, Color.Yellow);
        _popupSystem.PopupClient(Loc.GetString("thermite-on-structure-ignited"), args.User, args.User, PopupType.MediumCaution);
        ent.Comp.Ignited = true;
        TryPlayBurningSound(ent.Owner, ent.Comp);
        ent.Comp.IgnitionStartTime = (float) _gameTiming.CurTime.TotalSeconds;
        ent.Comp.ThermiteSolutionComponent.Solution.RemoveAllSolution();
        return true;
    }

    // Secondary Chemistry
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
}
