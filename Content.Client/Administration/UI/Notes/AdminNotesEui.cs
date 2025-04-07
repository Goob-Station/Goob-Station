// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Eui;
using Content.Shared.Administration.Notes;
using Content.Shared.Eui;
using JetBrains.Annotations;
using static Content.Shared.Administration.Notes.AdminNoteEuiMsg;

namespace Content.Client.Administration.UI.Notes;

[UsedImplicitly]
public sealed class AdminNotesEui : BaseEui
{
    public AdminNotesEui()
    {
        NoteWindow = new AdminNotesWindow();
        NoteControl = NoteWindow.Notes;

        NoteControl.NoteChanged += (id, type, text, severity, secret, expiryTime) => SendMessage(new EditNoteRequest(id, type, text, severity, secret, expiryTime));
        NoteControl.NewNoteEntered += (type, text, severity, secret, expiryTime) => SendMessage(new CreateNoteRequest(type, text, severity, secret, expiryTime));
        NoteControl.NoteDeleted += (id, type) => SendMessage(new DeleteNoteRequest(id, type));
        NoteWindow.OnClose += () => SendMessage(new CloseEuiMessage());
    }

    public override void Closed()
    {
        base.Closed();
        NoteWindow.Close();
    }

    private AdminNotesWindow NoteWindow { get; }

    private AdminNotesControl NoteControl { get; }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not AdminNotesEuiState s)
        {
            return;
        }

        NoteWindow.SetTitlePlayer(s.NotedPlayerName);
        NoteControl.SetPlayerName(s.NotedPlayerName);
        NoteControl.SetNotes(s.Notes);
        NoteControl.SetPermissions(s.CanCreate, s.CanDelete, s.CanEdit);
    }

    public override void Opened()
    {
        NoteWindow.OpenCentered();
    }
}