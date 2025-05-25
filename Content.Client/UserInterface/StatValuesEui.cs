// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared.UserInterface;

namespace Content.Client.UserInterface;

public sealed class StatValuesEui : BaseEui
{
    private readonly StatsWindow _window;

    public StatValuesEui()
    {
        _window = new StatsWindow();
        _window.Title = "Melee stats";
        _window.OpenCentered();
        _window.OnClose += Closed;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not StatValuesEuiMessage eui)
            return;

        _window.Title = eui.Title;
        _window.UpdateValues(eui.Headers, eui.Values);
    }
}