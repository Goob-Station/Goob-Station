// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared._White.Standing;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Clothing.Systems;

public sealed class MultiplyStandingUpTimeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModifyStandingUpTimeComponent, GetStandingUpTimeMultiplierEvent>(OnGetMultiplier);
        SubscribeLocalEvent<ModifyStandingUpTimeComponent, InventoryRelayedEvent<GetStandingUpTimeMultiplierEvent>>(
            OnInventoryGetMultiplier);
        SubscribeLocalEvent<ModifyStandingUpTimeComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<ModifyStandingUpTimeComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<ClothingComponent>(ent))
            return;

        var msg = Loc.GetString("clothing-modify-stand-up-time-examine",
            ("mod", MathF.Round((1f - ent.Comp.Multiplier) * 100)));
        args.PushMarkup(msg);
    }

    private void OnInventoryGetMultiplier(Entity<ModifyStandingUpTimeComponent> ent, ref InventoryRelayedEvent<GetStandingUpTimeMultiplierEvent> args)
    {
        args.Args.Multiplier *= ent.Comp.Multiplier;
    }

    private void OnGetMultiplier(Entity<ModifyStandingUpTimeComponent> ent, ref GetStandingUpTimeMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }
}