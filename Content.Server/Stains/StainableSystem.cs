// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Forensics;
using Content.Shared.Chemistry.Components;
using Content.Shared.Forensics.Components;
using Content.Shared.Stains;
using Content.Shared.Tag;
using Robust.Shared.Containers; // Gaby

namespace Content.Server.Stains;

public sealed partial class StainableSystem : SharedStainableSystem
{
    [Dependency] private readonly ForensicsSystem _forensics = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!; // Gaby

    protected override void DirtyOwnerAppearance(EntityUid item) // Gaby
    {
        if (_container.TryGetContainingContainer(item, out var container) && TryComp<AppearanceComponent>(container.Owner, out var appearance))
        {
            Dirty(container.Owner, appearance);
        }
    }

    protected override void StainForensics(Entity<StainableComponent> ent, Entity<SolutionComponent> solution)
    {
        _tag.AddTag(ent.Owner, ForensicScannerSystem.DNASolutionScannableTag);
    }

    protected override void WashingForensics(Entity<StainableComponent> ent, Entity<SolutionComponent> solution, EntityUid washingMachine)
    {
        if (!TryComp<ForensicsComponent>(washingMachine, out var forensics))
            return;

        forensics.DNAs.UnionWith(_forensics.GetSolutionsDNA(solution.Comp.Solution));
    }
}
