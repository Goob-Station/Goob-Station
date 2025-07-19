// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Seven2280 <semvalentin123@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Changeling.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Nutrition.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition;
using Content.Shared.Tag;
using Robust.Shared.GameObjects;
using Content.Goobstation.Server.Changeling;

namespace Content.Goobstation.Server.Changeling;

public sealed class ChangelingOrganDigestionSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly ChangelingSystem _changeling = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FoodComponent, BeforeFullyEatenEvent>(OnBeforeFullyEaten);
    }

    private void OnBeforeFullyEaten(EntityUid uid, FoodComponent component, BeforeFullyEatenEvent args)
    {
        if (!TryComp<ChangelingOrganDigestionComponent>(args.User, out var digestion)
            || !_tag.HasTag(uid, digestion.DigestibleTag))
            return;

        if (TryComp<ChangelingIdentityComponent>(args.User, out var lingComp))
            _changeling.UpdateChemicals(args.User, lingComp, digestion.ChemicalsPerItem);
    }
}
