// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Preferences
{
    /// <summary>
    ///     Specifies behavior when none of the jobs you want are available at round start.
    /// </summary>
    public enum PreferenceUnavailableMode
    {
        // These enum values HAVE to match the ones in DbPreferenceUnavailableMode in Server.Database.

        /// <summary>
        ///     Stay in the lobby (if the lobby is enabled).
        /// </summary>
        StayInLobby = 0,

        /// <summary>
        ///     Spawn as overflow role if preference unavailable.
        /// </summary>
        SpawnAsOverflow,
    }
}