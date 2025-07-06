using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Tools.Components;
using Content.Shared.Whitelist;

namespace Content.Shared.Tools.Systems;

public abstract partial class SharedToolSystem
{
    public void InitializeSolutionRefuel()
    {
        SubscribeLocalEvent<SolutionRefuelComponent, ExaminedEvent>(SolutionRefuelExamine);
        SubscribeLocalEvent<SolutionRefuelComponent, AfterInteractEvent>(OnSolutionRefuelAfterInteract);
    }

    private string GetName(Entity<SolutionRefuelComponent> entity)
    {
        if (entity.Comp.Name == null)
            return Identity.Name(entity, EntityManager);
        return Loc.GetString(entity.Comp.Name);
    }

    public bool TryGetSolutionFuelAndCapacity(EntityUid uid, out FixedPoint2 fuel, out FixedPoint2 capacity, SolutionRefuelComponent? welder = null, SolutionContainerManagerComponent? solutionContainer = null)
    {
        fuel = default;
        capacity = default;
        if (!Resolve(uid, ref welder, ref solutionContainer))
            return false;

        if (!SolutionContainerSystem.TryGetSolution(
                (uid, solutionContainer),
                welder.FuelSolutionName,
                out _,
                out var fuelSolution))
            return false;

        fuel = fuelSolution.GetTotalPrototypeQuantity(welder.FuelReagent);
        capacity = fuelSolution.MaxVolume;
        return true;
    }

    private void SolutionRefuelExamine(Entity<SolutionRefuelComponent> entity, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(SolutionRefuelComponent)))
        {
            if (args.IsInDetailsRange)
            {
                var success = TryGetSolutionFuelAndCapacity(entity.Owner, out var fuel, out var capacity);

                args.PushMarkup(Loc.GetString("solution-refuel-component-on-examine-detailed-message",
                    ("colorName", fuel < capacity / FixedPoint2.New(4f) ? "darkorange" : "orange"),
                    ("fuelLeft", fuel),
                    ("fuelCapacity", capacity)));
            }
        }
    }

    private void OnSolutionRefuelAfterInteract(Entity<SolutionRefuelComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target is not { Valid: true } target || !args.CanReach)
            return;

        if (TryComp(target, out ReagentTankComponent? tank)
            && tank.TankType == ReagentTankType.Fuel
            && SolutionContainerSystem.TryGetDrainableSolution(target, out var targetSoln, out var targetSolution)
            && _whitelist.CheckBoth(entity, tank.FuelBlacklist, tank.FuelWhitelist)
            && SolutionContainerSystem.TryGetSolution(entity.Owner, entity.Comp.FuelSolutionName, out var solutionComp, out var welderSolution))
        {
            var name = GetName(entity);
            var trans = FixedPoint2.Min(welderSolution.AvailableVolume, targetSolution.Volume);
            if (trans > 0)
            {
                var drained = SolutionContainerSystem.Drain(target, targetSoln.Value, trans);
                SolutionContainerSystem.TryAddSolution(solutionComp.Value, drained);
                _audioSystem.PlayPredicted(entity.Comp.WelderRefill, entity, user: args.User);
                _popup.PopupClient(Loc.GetString("welder-component-after-interact-refueled-message"), entity, args.User);
            }
            else if (welderSolution.AvailableVolume <= 0)
            {
                _popup.PopupClient(Loc.GetString("solution-refuel-component-already-full", ("name", name)), entity, args.User);
            }
            else
            {
                _popup.PopupClient(Loc.GetString("welder-component-no-fuel-in-tank", ("owner", args.Target)), entity, args.User);
            }

            args.Handled = true;
        }
    }
}
