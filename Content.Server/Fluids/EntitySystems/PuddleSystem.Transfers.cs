// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TNE <38938720+JustTNE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Topy <topy72.mine@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.Components;
using Content.Shared.DragDrop;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;

namespace Content.Server.Fluids.EntitySystems;

public sealed partial class PuddleSystem
{
    private void InitializeTransfers()
    {
        SubscribeLocalEvent<RefillableSolutionComponent, DragDropDraggedEvent>(OnRefillableDragged);
    }

    private void OnRefillableDragged(Entity<RefillableSolutionComponent> entity, ref DragDropDraggedEvent args)
    {
        if (!_solutionContainerSystem.TryGetSolution(entity.Owner, entity.Comp.Solution, out var soln, out var solution) || solution.Volume == FixedPoint2.Zero)
        {
            _popups.PopupEntity(Loc.GetString("mopping-system-empty", ("used", entity.Owner)), entity, args.User);
            return;
        }

        // Dump reagents into DumpableSolution
        if (TryComp<DumpableSolutionComponent>(args.Target, out var dump))
        {
            if (!_solutionContainerSystem.TryGetDumpableSolution((args.Target, dump, null), out var dumpableSoln, out var dumpableSolution))
                return;

            bool success = true;
            if (dump.Unlimited)
            {
                var split = _solutionContainerSystem.SplitSolution(soln.Value, solution.Volume);
                dumpableSolution.AddSolution(split, _prototypeManager);
            }
            else
            {
                var split = _solutionContainerSystem.SplitSolution(soln.Value, dumpableSolution.AvailableVolume);
                success = _solutionContainerSystem.TryAddSolution(dumpableSoln.Value, split);
            }

            if (success)
            {
                _audio.PlayPvs(AbsorbentComponent.DefaultTransferSound, args.Target);
            }
            else
            {
                _popups.PopupEntity(Loc.GetString("mopping-system-full", ("used", args.Target)), args.Target, args.User);
            }

            return;
        }

        // Take reagents from target
        if (!TryComp<DrainableSolutionComponent>(args.Target, out var drainable))
        {
            if (!_solutionContainerSystem.TryGetDrainableSolution((args.Target, drainable, null), out var drainableSolution, out _))
                return;

            var split = _solutionContainerSystem.SplitSolution(drainableSolution.Value, solution.AvailableVolume);

            if (_solutionContainerSystem.TryAddSolution(soln.Value, split))
            {
                _audio.PlayPvs(AbsorbentComponent.DefaultTransferSound, entity);
            }
            else
            {
                _popups.PopupEntity(Loc.GetString("mopping-system-full", ("used", entity.Owner)), entity, args.User);
            }
        }
    }
}