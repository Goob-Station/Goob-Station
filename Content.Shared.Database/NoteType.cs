// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Database;

/*
 * Editing the numbers here may obliterate DB records, you have been warned.
 * If you do have to edit the numbers for some reason, please create migrations.
 * Adding new types is fine (or even renaming), but do not remove or change them.
 */

/// <summary>
///     Different types of notes
/// </summary>
public enum NoteType
{
    /// <summary>
    ///     Normal note
    /// </summary>
    Note = 0,

    /// <summary>
    ///     Watchlist, a secret note that gets shown to online admins every time a player connects
    /// </summary>
    Watchlist = 1,

    /// <summary>
    ///     A message, type of note that gets explicitly shown to the player
    /// </summary>
    Message = 2,

    /// <summary>
    ///     A server ban, converted to a shared note
    /// </summary>
    ServerBan = 3,

    /// <summary>
    ///     A role ban, converted to a shared note
    /// </summary>
    RoleBan = 4,
}