// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Goobstation.Shared.Chemistry.GunApplySolution;

public sealed class GunApplySolutionSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GunApplySolutionComponent, GunShotEvent>(OnGunShot);
    }

    private void OnGunShot(EntityUid uid, GunApplySolutionComponent comp, ref GunShotEvent args)
    {
        if (!HasComp<SolutionContainerManagerComponent>(uid)
        || !_solutionContainer.TryGetSolution(uid, comp.SourceSolution, out var ent, out Solution? source)
        || ent == null
        || source == null)
            return;

        foreach (var (ammo, _) in args.Ammo) // This gives wrong uid on client
        {
            if (!TryComp<SolutionContainerManagerComponent>(ammo, out var solution)
            || !_solutionContainer.TryGetSolution(solution.Owner, comp.TargetSolution, out var target)
            || target == null)
                continue;

            _solutionContainer.TryTransferSolution(target.Value, source, comp.Amount);
        }

        _solutionContainer.UpdateChemicals(ent.Value); // So we call this
    }
}
