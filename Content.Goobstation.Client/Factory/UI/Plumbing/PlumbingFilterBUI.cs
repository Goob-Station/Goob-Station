// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory.Plumbing;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Factory.UI.Plumbing;

public sealed class PlumbingFilterBUI : BoundUserInterface
{
    private PlumbingFilterWindow? _window;

    public PlumbingFilterBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PlumbingFilterWindow>();
        _window.SetEntity(Owner);
        _window.OnChange += id => SendPredictedMessage(new PlumbingFilterChangeMessage(id));
    }
}
