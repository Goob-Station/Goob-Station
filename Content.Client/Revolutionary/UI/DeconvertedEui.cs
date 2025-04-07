// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: MIT

using Content.Client.Eui;

namespace Content.Client.Revolutionary.UI;

public sealed class DeconvertedEui : BaseEui
{
    private readonly DeconvertedMenu _menu;

    public DeconvertedEui()
    {
        _menu = new DeconvertedMenu();
    }

    public override void Opened()
    {
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }
}