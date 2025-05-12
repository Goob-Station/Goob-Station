// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;
using Content.Shared.Heretic.Messages;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client._Shitcode.Heretic.UI;

[UsedImplicitly]
public sealed class FeastOfOwlsEui : BaseEui
{
    private readonly FeastOfOwlsMenu _menu;

    public FeastOfOwlsEui()
    {
        _menu = new FeastOfOwlsMenu();

        _menu.DenyButton.OnPressed += _ =>
        {
            SendMessage(new FeastOfOwlsMessage(false));
            _menu.Close();
        };

        _menu.AcceptButton.OnPressed += _ =>
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
