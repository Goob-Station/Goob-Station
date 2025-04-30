// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;
using Content.Shared.Administration.Notes;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client.Administration.UI.AdminRemarks;

[UsedImplicitly]
public sealed class UserNotesEui : BaseEui
{
    public UserNotesEui()
    {
        NoteWindow = new AdminRemarksWindow();
        NoteWindow.OnClose += () => SendMessage(new CloseEuiMessage());
    }

    private AdminRemarksWindow NoteWindow { get; }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not UserNotesEuiState s)
        {
            return;
        }

        NoteWindow.SetNotes(s.Notes);
    }

    public override void Opened()
    {
        NoteWindow.OpenCentered();
    }
}