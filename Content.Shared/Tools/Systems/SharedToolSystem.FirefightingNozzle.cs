// Assmos - Extinguisher Nozzle

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Tools.Components;
using Content.Shared.Inventory; 
using Content.Shared.Whitelist; 

namespace Content.Shared.Tools.Systems;

public abstract partial class SharedToolSystem
{
    [Dependency] private readonly InventorySystem _inventory = default!; 
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!; 
    public void InitializeFirefightingNozzle()
    {
        SubscribeLocalEvent<FirefightingNozzleComponent, AfterInteractEvent>(OnFirefightingNozzleAfterInteract);
    }

    private void OnFirefightingNozzleAfterInteract(Entity<FirefightingNozzleComponent> entity, ref AfterInteractEvent args)
    {
        var sprayOwner = entity.Owner;
        var solutionName = FirefightingNozzleComponent.SolutionName;

        if (args.Handled)
            return;

        if (args.Target is not { Valid: true } target || !args.CanReach)
            return;

        if (TryComp(target, out ReagentTankComponent? tank) && tank.TankType == ReagentTankType.Fuel)
            return;

        if (entity.Comp.ExternalContainer == true)
        {
            if (!_inventory.TryGetContainerSlotEnumerator(args.User, out var enumerator, entity.Comp.TargetSlot)) return;
                while (enumerator.NextItem(out var item))
                {
                    if (_whitelistSystem.IsWhitelistFailOrNull(entity.Comp.ProviderWhitelist, item)) continue;
                    sprayOwner = item;
                    solutionName = FirefightingNozzleComponent.SolutionName;
                }
        }

        if (SolutionContainerSystem.TryGetDrainableSolution(target, out var targetSoln, out var targetSolution)
            && SolutionContainerSystem.TryGetSolution(sprayOwner, solutionName, out var solutionComp, out var atmosBackpackTankSolution))
        {
            var trans = FixedPoint2.Min(atmosBackpackTankSolution.AvailableVolume, targetSolution.Volume);
            if (trans > 0)
            {
                var drained = SolutionContainerSystem.Drain(target, targetSoln.Value, trans);
                SolutionContainerSystem.TryAddSolution(solutionComp.Value, drained);
                _audioSystem.PlayPredicted(entity.Comp.FirefightingNozzleRefill, entity, user: args.User);
                _popup.PopupClient(Loc.GetString("firefighter-nozzle-component-after-interact-refilled-message"), entity, args.User);
            }
            else if (atmosBackpackTankSolution.AvailableVolume <= 0)
            {
                _popup.PopupClient(Loc.GetString("firefighter-nozzle-component-already-full"), entity, args.User);
            }
            else
            {
                _popup.PopupClient(Loc.GetString("firefighter-nozzle-component-no-water-in-tank", ("owner", args.Target)), entity, args.User);
            }

            args.Handled = true;
        }
    }
}
