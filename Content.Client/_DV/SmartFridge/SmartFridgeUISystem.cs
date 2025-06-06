// SPDX-FileCopyrightText: 2025 Jarl <elliotacline@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.SmartFridge;
using Robust.Shared.Analyzers;

namespace Content.Client._DV.SmartFridge;

public sealed class SmartFridgeUISystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartFridgeComponent, AfterAutoHandleStateEvent>(OnSmartFridgeAfterState);
    }

    private void OnSmartFridgeAfterState(Entity<SmartFridgeComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!_uiSystem.TryGetOpenUi<SmartFridgeBoundUserInterface>(ent.Owner, SmartFridgeUiKey.Key, out var bui))
            return;

        bui.Refresh();
    }
}
