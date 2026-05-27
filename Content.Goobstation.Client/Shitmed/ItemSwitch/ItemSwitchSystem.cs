// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Items;
using Content.Goobstation.Shared.Shitmed.ItemSwitch;
using Content.Goobstation.Shared.Shitmed.ItemSwitch.Components;
using Content.Goobstation.Client.Shitmed.ItemSwitch.UI;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Shitmed.ItemSwitch;

public sealed class ItemSwitchSystem : SharedItemSwitchSystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<ItemSwitchComponent>(ent => new ItemSwitchStatusControl(ent));
        SubscribeLocalEvent<ItemSwitchComponent, AfterAutoHandleStateEvent>(OnChanged);
    }

    private void OnChanged(Entity<ItemSwitchComponent> ent, ref AfterAutoHandleStateEvent args) => UpdateVisuals(ent, ent.Comp.State);

    protected override void UpdateVisuals(Entity<ItemSwitchComponent> ent, string key)
    {
        base.UpdateVisuals(ent, key);
        if (TryComp(ent, out SpriteComponent? sprite) && ent.Comp.States.TryGetValue(key, out var state))
            if (state.Sprite != null)
                sprite.LayerSetSprite(0, state.Sprite);
    }
}