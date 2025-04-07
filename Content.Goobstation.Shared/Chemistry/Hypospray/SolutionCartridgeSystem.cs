// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Chemistry.Hypospray;

public sealed class SolutionCartridgeSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HyposprayComponent, EntInsertedIntoContainerMessage>(OnCartridgeInserted);
        SubscribeLocalEvent<HyposprayComponent, EntRemovedFromContainerMessage>(OnCartridgeRemoved);
        SubscribeLocalEvent<HyposprayComponent, AfterHyposprayInjectsEvent>(OnHyposprayInjected);
    }

    private void OnCartridgeInserted(Entity<HyposprayComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<SolutionCartridgeComponent>(args.Entity, out var cartridge)
        || !TryComp(ent, out SolutionContainerManagerComponent? manager)
        || !_solution.TryGetSolution((ent, manager), cartridge.TargetSolution, out var solutionEntity))
            return;

        _solution.TryAddSolution(solutionEntity.Value, cartridge.Solution);
    }

    private void OnCartridgeRemoved(Entity<HyposprayComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!TryComp<SolutionCartridgeComponent>(args.Entity, out var cartridge)
        || !TryComp(ent, out SolutionContainerManagerComponent? manager)
        || !_solution.TryGetSolution((ent, manager), cartridge.TargetSolution, out var solutionEntity))
            return;

        _solution.RemoveAllSolution(solutionEntity.Value);
    }

    private void OnHyposprayInjected(Entity<HyposprayComponent> ent, ref AfterHyposprayInjectsEvent args)
    {
        if (!_container.TryGetContainer(ent, "item", out var container))
            return;

        _container.CleanContainer(container);
    }
}