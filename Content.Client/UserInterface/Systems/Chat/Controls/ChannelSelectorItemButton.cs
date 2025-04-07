// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Content.Client.Stylesheets;
using Content.Shared.Chat;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Systems.Chat.Controls;

public sealed class ChannelSelectorItemButton : Button
{
    public readonly ChatSelectChannel Channel;

    public bool IsHidden => Parent == null;

    public ChannelSelectorItemButton(ChatSelectChannel selector)
    {
        Channel = selector;
        AddStyleClass(StyleNano.StyleClassChatChannelSelectorButton);

        Text = ChannelSelectorButton.ChannelSelectorName(selector);

        var prefix = ChatUIController.ChannelPrefixes[selector];

        if (prefix != default)
            Text = Loc.GetString("hud-chatbox-select-name-prefixed", ("name", Text), ("prefix", prefix));
    }
}