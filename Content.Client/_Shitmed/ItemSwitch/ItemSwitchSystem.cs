// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Items;
using Content.Shared._Shitmed.ItemSwitch;
using Content.Shared._Shitmed.ItemSwitch.Components;
using Content.Client._Shitmed.ItemSwitch.UI;
using Robust.Client.GameObjects;

namespace Content.Client._Shitmed.ItemSwitch;

public sealed class ItemSwitchSystem : SharedItemSwitchSystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<ItemSwitchComponent>(ent => new ItemSwitchStatusControl(ent));
        SubscribeLocalEvent<ItemSwitchComponent, AfterAutoHandleStateEvent>(OnChanged);
    }

    private void OnChanged(Entity<ItemSwitchComponent> ent, ref AfterAutoHandleStateEvent args) // Pirate: katana
    {
        UpdateVisuals(ent, ent.Comp.State);
        RaiseReplicatedItemSwitched(ent); // Pirate: katana
    }

    #region Pirate: katana
    private void RaiseReplicatedItemSwitched(Entity<ItemSwitchComponent> ent)
    {
        var switched = new ItemSwitchedEvent
        {
            Predicted = false,
            State = ent.Comp.State,
            User = null,
        };

        RaiseLocalEvent(ent, ref switched);
    }
    #endregion Pirate: katana

    protected override void UpdateVisuals(Entity<ItemSwitchComponent> ent, string key)
    {
        base.UpdateVisuals(ent, key);
        if (TryComp(ent, out SpriteComponent? sprite) && ent.Comp.States.TryGetValue(key, out var state))
            if (state.Sprite != null)
                sprite.LayerSetSprite(0, state.Sprite);
    }
}
