// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;
using Content.Goobstation.Client.UserInterface;
using Content.Shared.Heretic.Messages;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Heretic.UI;

public sealed class FeastOfOwlsEui : BaseEui
{
    private readonly SimpleConfirmationMenu _menu;

    public FeastOfOwlsEui()
    {
        _menu = new SimpleConfirmationMenu("feast-of-owls-text", "feast-of-owls-accept-button", "feast-of-owls-deny-button");

        _menu.CancelButton.OnPressed += _ =>
        {
            SendMessage(new FeastOfOwlsMessage(false));
            _menu.Close();
        };

        _menu.ConfirmButton.OnPressed += _ =>
        {
            SendMessage(new FeastOfOwlsMessage(true));
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

        SendMessage(new FeastOfOwlsMessage(false));
        _menu.Close();
    }

}
