// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration;
using Content.Shared.Administration.Managers;
using Robust.Shared.Player;
using Robust.Shared.Toolshed;

namespace Content.Server.Administration.Managers
{
    /// <summary>
    ///     Manages server administrators and their permission flags.
    /// </summary>
    public interface IAdminManager : ISharedAdminManager
    {
        /// <summary>
        ///     Fired when the permissions of an admin on the server changed.
        /// </summary>
        event Action<AdminPermsChangedEventArgs> OnPermsChanged;

        /// <summary>
        ///     Gets all active admins currently on the server.
        /// </summary>
        /// <remarks>
        ///     This does not include admins that are de-adminned.
        /// </remarks>
        IEnumerable<ICommonSession> ActiveAdmins { get; }

        /// <summary>
        /// Gets all admins currently on the server, even de-adminned ones.
        /// </summary>
        IEnumerable<ICommonSession> AllAdmins { get; }

        /// <summary>
        ///     De-admins an admin temporarily so they are effectively a normal player.
        /// </summary>
        /// <remarks>
        ///     De-adminned admins are able to re-admin at any time if they so desire.
        /// </remarks>
        void DeAdmin(ICommonSession session);

        /// <summary>
        ///     Re-admins a de-adminned admin.
        /// </summary>
        void ReAdmin(ICommonSession session);

        /// <summary>
        ///     Make admin hidden from adminwho.
        /// </summary>
        void Stealth(ICommonSession session);

        /// <summary>
        ///     Unhide admin from adminwho.
        /// </summary>
        void UnStealth(ICommonSession session);

        /// <summary>
        ///     Re-loads the permissions of an player in case their admin data changed DB-side.
        /// </summary>
        /// <seealso cref="ReloadAdminsWithRank"/>
        void ReloadAdmin(ICommonSession player);

        /// <summary>
        ///     Reloads admin permissions for all admins with a certain rank.
        /// </summary>
        /// <param name="rankId">The database ID of the rank.</param>
        /// <seealso cref="ReloadAdmin"/>
        void ReloadAdminsWithRank(int rankId);

        void Initialize();

        void PromoteHost(ICommonSession player);

        bool TryGetCommandFlags(CommandSpec command, out AdminFlags[]? flags);
    }
}