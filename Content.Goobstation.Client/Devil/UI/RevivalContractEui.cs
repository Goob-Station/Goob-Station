// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;
using Content.Goobstation.Shared.Devil;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Devil.UI;

[UsedImplicitly]
public sealed class RevivalContractEui : BaseEui
{
    private readonly RevivalContractMenu _menu;

    public RevivalContractEui()
    {
        _menu = new RevivalContractMenu();

        _menu.DenyButton.OnPressed += _ =>
        {
            SendMessage(new RevivalContractMessage(false));
            _menu.Close();
        };

        _menu.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new RevivalContractMessage(true));
            _menu.Close();
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        SendMessage(new RevivalContractMessage(false));
        _menu.Close();
    }

}
