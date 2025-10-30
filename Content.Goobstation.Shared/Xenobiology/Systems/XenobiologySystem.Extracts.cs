// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Chemistry;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Examine;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

// This handles slime extracts.
public partial class XenobiologySystem
{
    private void SubscribeExtracts()
    {
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
