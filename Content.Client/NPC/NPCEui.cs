// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Eui;
using Content.Shared.Eui;

namespace Content.Client.NPC;

public sealed class NPCEui : BaseEui
{
    private NPCWindow? _window = new();

    public override void Opened()
    {
        base.Opened();
        _window = new NPCWindow();
        _window.OpenCentered();
        _window.OnClose += OnClosed;
    }

    private void OnClosed()
    {
        SendMessage(new CloseEuiMessage());
    }

    public override void Closed()
    {
        base.Closed();
        _window?.Close();
        _window = null;
    }
}