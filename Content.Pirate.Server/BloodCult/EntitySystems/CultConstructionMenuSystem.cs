// SPDX-FileCopyrightText: 2026 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._White.RadialSelector;
using Content.Shared.BloodCult.Components;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class CultConstructionMenuSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultConstructionMenuComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpen);
    }

    private void OnBeforeOpen(Entity<CultConstructionMenuComponent> menu, ref BeforeActivatableUIOpenEvent args)
    {
        _ui.SetUiState(
            menu.Owner,
            RadialSelectorUiKey.Key,
            new RadialSelectorState(menu.Comp.Entries));
    }
}
