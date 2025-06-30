// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration;
using Robust.Shared.Player;

namespace Content.Server.Administration
{
    /// <summary>
    ///     Sealed when the permissions of an admin on the server change.
    /// </summary>
    public sealed class AdminPermsChangedEventArgs : EventArgs
    {
        public AdminPermsChangedEventArgs(ICommonSession player, AdminFlags? flags)
        {
            Player = player;
            Flags = flags;
        }

        /// <summary>
        ///     The player that had their admin permissions changed.
        /// </summary>
        public ICommonSession Player { get; }

        /// <summary>
        ///     The admin flags of the player. Null if the player is no longer an admin.
        /// </summary>
        public AdminFlags? Flags { get; }

        /// <summary>
        ///     Whether the player is now an admin.
        /// </summary>
        public bool IsAdmin => Flags.HasValue;
    }
}