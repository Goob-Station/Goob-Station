// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Items.UI;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Robust.Client.UserInterface.Controls;
using Content.Shared._Shitmed.ItemSwitch.Components;

namespace Content.Client._Shitmed.ItemSwitch.UI;

public sealed class ItemSwitchStatusControl : PollingItemStatusControl<ItemSwitchStatusControl.Data>
{
    private readonly Entity<ItemSwitchComponent> _parent;
    private readonly RichTextLabel _label;

    public ItemSwitchStatusControl(Entity<ItemSwitchComponent> parent)
    {
        _parent = parent;
        _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
        if (parent.Comp.ShowLabel)
            AddChild(_label);

        UpdateDraw();
    }

    protected override Data PollData()
    {
        return new Data(_parent.Comp.State);
    }

    protected override void Update(in Data data)
    {
        _label.SetMarkup(Loc.GetString("itemswitch-component-on-examine-detailed-message",
            ("state", data.State)));
    }

    public record struct Data(string State);
}