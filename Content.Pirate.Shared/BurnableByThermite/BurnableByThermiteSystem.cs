using System.Diagnostics;
using System.Linq;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.HealthExaminable;
using Content.Shared.IgnitionSource;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;

namespace Content.Pirate.Shared.BurnableByThermite;

public sealed class SharedBurnableByThermiteSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BurnableByThermiteComponent, InteractUsingEvent>(OnInteract);
    }
    public void OnInteract(EntityUid uid, BurnableByThermiteComponent component, InteractUsingEvent args)
    {
        if (!HasComp<DamageableComponent>(uid)) return; // Don't bother if it's not damageable
        if (TryComp<SolutionContainerManagerComponent>(args.Used, out var beakerSolutionContainerComponent))
            OnInteractWithBeaker(uid, args.Used, args, beakerSolutionContainerComponent);

        // TryComp<IgnitionSourceComponent>(args.Used, out var ignitionSourceComponent);

    }
    public void OnInteractWithBeaker(EntityUid structure, EntityUid beaker, InteractUsingEvent args, SolutionContainerManagerComponent beakerSolutionContainerComponent)
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
            if (structureThermiteSolution is null) return;  // Just in case

            if (thermiteReagent.Quantity < structureThermiteSolution.MaxVolume)
            {
                _popupSystem.PopupClient(Loc.GetString("thermite-on-structure-not-enough"), args.User, args.User, PopupType.Small);
                continue;
            }
            if (thermiteReagent.Quantity >= structureThermiteSolution.MaxVolume)
            {

                _solutionSystem.TryAddReagent(structureThermiteSolutionEntity.Value, thermiteReagent, out var transferedAmount);
                if (transferedAmount == 0)
                {
                    _popupSystem.PopupClient(Loc.GetString("thermite-on-structure-full"), args.User, args.User, PopupType.Medium);
                    return;
                }
                else
                {
                    _popupSystem.PopupClient(Loc.GetString("thermite-on-structure-success"), args.User, args.User, PopupType.MediumCaution);
                    _solutionSystem.RemoveReagent(solutionEntity, "Thermite", transferedAmount);
                    return;
                }
            }
        }

    }

}
