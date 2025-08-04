// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;
using Content.Goobstation.Shared.Heretic.Messages;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Heretic.Heretic.UI;

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
