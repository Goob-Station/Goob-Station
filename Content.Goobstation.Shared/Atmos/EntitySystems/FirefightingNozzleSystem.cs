// Assmos - Extinguisher Nozzle

using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;
using Content.Goobstation.Shared.Atmos.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Atmos.Systems;

public sealed class FirefightingNozzleSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
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
            if (!_inventory.TryGetContainerSlotEnumerator(args.User, out var enumerator, entity.Comp.TargetSlot))
                return;
            while (enumerator.NextItem(out var item))
            {
                if (_whitelistSystem.IsWhitelistFailOrNull(entity.Comp.ProviderWhitelist, item)) continue;
                sprayOwner = item;
                solutionName = FirefightingNozzleComponent.SolutionName;
            }
        }

        if (_solutionContainerSystem.TryGetDrainableSolution(target, out var targetSoln, out var targetSolution)
            && _solutionContainerSystem.TryGetSolution(sprayOwner, solutionName, out var solutionComp, out var atmosBackpackTankSolution))
        {
            var trans = FixedPoint2.Min(atmosBackpackTankSolution.AvailableVolume, targetSolution.Volume);
            if (trans > 0)
            {
                var drained = _solutionContainerSystem.Drain(target, targetSoln.Value, trans);
                _solutionContainerSystem.TryAddSolution(solutionComp.Value, drained);
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
