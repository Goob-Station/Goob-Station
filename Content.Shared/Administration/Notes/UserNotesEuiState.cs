// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Database;
using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.Notes;

[Serializable, NetSerializable]
public sealed class UserNotesEuiState : EuiStateBase
{
    public UserNotesEuiState(Dictionary<(int, NoteType), SharedAdminNote> notes)
    {
        Notes = notes;
    }
    public Dictionary<(int, NoteType), SharedAdminNote> Notes { get; }
}
