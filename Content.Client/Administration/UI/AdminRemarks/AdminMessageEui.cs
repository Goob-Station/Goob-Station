// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Eui;
using Content.Shared.Administration.Notes;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using static Content.Shared.Administration.Notes.AdminMessageEuiMsg;

namespace Content.Client.Administration.UI.AdminRemarks;

[UsedImplicitly]
public sealed class AdminMessageEui : BaseEui
{
    private readonly AdminMessagePopupWindow _popup;

    public AdminMessageEui()
    {
        _popup = new AdminMessagePopupWindow();
        _popup.OnAcceptPressed += () => SendMessage(new Dismiss(true));
        _popup.OnDismissPressed += () => SendMessage(new Dismiss(false));
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not AdminMessageEuiState s)
        {
            return;
        }

        _popup.SetState(s);
    }

    public override void Opened()
    {
        _popup.UserInterfaceManager.WindowRoot.AddChild(_popup);
        LayoutContainer.SetAnchorPreset(_popup, LayoutContainer.LayoutPreset.Wide);
    }

    public override void Closed()
    {
        _popup.Orphan();
    }
}