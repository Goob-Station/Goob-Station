// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Chemistry;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

// This handles slime extracts.
public sealed partial class SlimeExtractSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeExtractComponent, BeforeSolutionReactEvent>(BeforeSolutionReact);
    }

    private void BeforeSolutionReact(Entity<SlimeExtractComponent> extract, ref BeforeSolutionReactEvent args)
    {
        // clean up the reagents inside when performing an effect
        if (_solution.TryGetRefillableSolution(extract.Owner, out var soln, out _))
            _solution.RemoveAllSolution((extract.Owner, soln));
    }
}
