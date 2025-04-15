// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Radio.Ui;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Radio.EntitySystems;

public sealed class RadioDeviceSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<IntercomComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
    }

    private void OnAfterHandleState(Entity<IntercomComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_ui.TryGetOpenUi<IntercomBoundUserInterface>(ent.Owner, IntercomUiKey.Key, out var bui))
            bui.Update(ent);
    }
}