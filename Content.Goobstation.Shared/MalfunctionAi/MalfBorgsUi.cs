// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MalfunctionAi;

[Serializable, NetSerializable]
public enum MalfBorgsUiKey : byte
{
    Key,
}

/// <summary>
/// State for the Malfunction AI's subverted-cyborgs window.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfBorgsBuiState : BoundUserInterfaceState
{
    public List<MalfBorgEntry> Borgs;

    public MalfBorgsBuiState(List<MalfBorgEntry> borgs)
    {
        Borgs = borgs;
    }
}

/// <summary>
/// One subverted cyborg row: name, whether it is still alive and where it was last seen.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfBorgEntry
{
    public string Name = string.Empty;
    public bool Alive;
    public string Location = string.Empty;
}
