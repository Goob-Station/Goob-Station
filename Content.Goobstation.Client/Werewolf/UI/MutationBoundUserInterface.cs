// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.UI;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Werewolf.UI;

public sealed class MutationBoundUserInterface : BoundUserInterface
{
    private MutationMenu? _menu;

    public MutationBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MutationMenu>();
        _menu.SetEntity(Owner);
        _menu.Closed += OnClosed;

        _menu.OpenCentered();
    }

    private void OnClosed()
    {
        SendPredictedMessage(new ClosedMessage());
        Close();
    }
}
