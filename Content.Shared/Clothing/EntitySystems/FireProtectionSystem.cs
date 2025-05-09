// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kosticia <kosticia46@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Armor;
using Content.Shared.Atmos;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Clothing.EntitySystems;

/// <summary>
/// Handles reducing fire damage when wearing clothing with <see cref="FireProtectionComponent"/>.
/// </summary>
public sealed class FireProtectionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireProtectionComponent, InventoryRelayedEvent<GetFireProtectionEvent>>(OnGetProtection);
        SubscribeLocalEvent<FireProtectionComponent, ArmorExamineEvent>(OnArmorExamine);
    }

    private void OnGetProtection(Entity<FireProtectionComponent> ent, ref InventoryRelayedEvent<GetFireProtectionEvent> args)
    {
        args.Args.Reduce(ent.Comp.Reduction);
    }

    private void OnArmorExamine(Entity<FireProtectionComponent> ent, ref ArmorExamineEvent args)
    {
        var value = MathF.Round((1f - ent.Comp.Reduction) * 100, 1);

        if (value == 0)
            return;

        args.Msg.PushNewline();
        args.Msg.AddMarkupOrThrow(Loc.GetString(ent.Comp.ExamineMessage, ("value", value)));
    }
}