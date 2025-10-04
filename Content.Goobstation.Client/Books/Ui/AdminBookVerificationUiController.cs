// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Lobby;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Books;
using Content.Shared.Heretic;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Books.Ui;

public sealed class AdminBookVerificationUiController : UIController
{
    private AdminBookVerificationMenu? _menu;

    public void ToggleMenu()
    {
        if (_menu != null)
        {
            _menu.Close();
            return;
        }

        _menu = new();
        _menu.OpenCentered();
        _menu.OnClose += () => _menu = null;
        _menu.ApproveBook += () =>
        {
            var ev = new ApproveBookMessage(_menu.Selected);
            EntityManager.RaisePredictiveEvent(ev);
        };
        _menu.DeclineBook += () =>
        {
            var ev = new DeclineBookMessage(_menu.Selected);
            EntityManager.RaisePredictiveEvent(ev);
        };
    }

    public void Populate(Dictionary<int, BookData> books)
    {
        _menu?.Populate(books);
    }
}
