// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Examine;

namespace Content.Goobstation.Server.Xenobiology.Extract;

/// <inheritdoc cref="SlimeExtractComponent"/>
public sealed partial class SlimeExtractSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeExtractComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SlimeExtractComponent, BeforeSolutionReactEvent>(BeforeSolutionReact);
    }

    private void OnExamined(Entity<SlimeExtractComponent> extract, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || !TryComp<ReactiveComponent>(extract, out var reactive))
            return;

        var message = Loc.GetString("slime-extract-examined-charges", ("num", reactive.RemainingReactions));
        if (reactive.IsReactionsUnlimited)
            message = Loc.GetString("slime-extract-examined-charges-infinite");

        args.PushMarkup(message);
    }
    private void BeforeSolutionReact(Entity<SlimeExtractComponent> extract, ref BeforeSolutionReactEvent args)
    {
        // clean up the reagents inside when performing an effect
        if (_solution.TryGetRefillableSolution(extract.Owner, out var soln, out _))
            _solution.RemoveAllSolution((extract.Owner, soln));
    }

}
