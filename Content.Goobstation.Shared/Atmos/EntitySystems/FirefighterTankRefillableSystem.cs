// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Assmos - Extinguisher Nozzle

using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;
using Content.Goobstation.Shared.Atmos.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Atmos.Systems;

public sealed class FirefighterTankRefillableSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FirefighterTankRefillableComponent, AfterInteractEvent>(OnFirefightingNozzleAfterInteract);
    }

    private void OnFirefightingNozzleAfterInteract(Entity<FirefighterTankRefillableComponent> entity, ref AfterInteractEvent args)
    {
        var sprayOwner = entity.Owner;
        var solutionName = FirefighterTankRefillableComponent.SolutionName;

        if (args.Handled)
            return;

        if (args.Target is not { Valid: true } target || !args.CanReach)
            return;

        if (TryComp(target, out ReagentTankComponent? tank) && tank.TankType == ReagentTankType.Fuel)
            return;

        if (entity.Comp.ExternalContainer)
        {
            bool foundContainer = false;

            // Check held items (exclude nozzle itself)
            foreach (var item in _handsSystem.EnumerateHeld(args.User))
            {
                if (item == entity.Owner)
                    continue;

                if (!_whitelistSystem.IsWhitelistFailOrNull(entity.Comp.ProviderWhitelist, item) &&
                    _solutionContainerSystem.TryGetSolution(item, FirefighterTankRefillableComponent.SolutionName, out _, out _))
                {
                    sprayOwner = item;
                    solutionName = FirefighterTankRefillableComponent.SolutionName;
                    foundContainer = true;
                    break;
                }
            }

            // Fall back to target slot
            if (!foundContainer && _inventory.TryGetContainerSlotEnumerator(args.User, out var enumerator, entity.Comp.TargetSlot))
            {
                while (enumerator.NextItem(out var item))
                {
                    if (!_whitelistSystem.IsWhitelistFailOrNull(entity.Comp.ProviderWhitelist, item) &&
                        _solutionContainerSystem.TryGetSolution(item, FirefighterTankRefillableComponent.SolutionName, out _, out _))
                    {
                        sprayOwner = item;
                        solutionName = FirefighterTankRefillableComponent.SolutionName;
                        foundContainer = true;
                        break;
                    }
                }
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
