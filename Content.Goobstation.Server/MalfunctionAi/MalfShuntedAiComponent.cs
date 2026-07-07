// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.MalfunctionAi;

/// <summary>
/// A Malfunction AI consciousness shunted into an APC. Holds a reference to the original
/// brain so "Return to Core" can transfer the mind back. The entity is parented to the APC,
/// so destroying the APC destroys the presence with it.
/// </summary>
[RegisterComponent]
public sealed partial class MalfShuntedAiComponent : Component
{
    /// <summary>
    /// The AI brain this consciousness came from.
    /// </summary>
    [DataField]
    public EntityUid? Brain;
}
