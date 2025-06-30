// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Geekyhobo <66805063+Ahlytlex@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Geekyhobo <66805063+Geekyhobo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Willis <143434770+CerberusWolfie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tainakov <136968973+Tainakov@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Administration
{
    /// <summary>
    ///     Permissions that admins can have.
    /// </summary>
    [Flags]
    public enum AdminFlags : uint
    {
        None = 0,

        /// <summary>
        ///     Basic admin verbs.
        /// </summary>
        Admin = 1 << 0,

        /// <summary>
        ///     Ability to ban people.
        /// </summary>
        Ban = 1 << 1,

        /// <summary>
        ///     Debug commands for coders.
        /// </summary>
        Debug = 1 << 2,

        /// <summary>
        ///     !!FUN!!
        ///     This is stuff that trial administrators shouldn't quite have access to yet, e.g. for running events.
        /// </summary>
        Fun = 1 << 3,

        /// <summary>
        ///     Ability to edit permissions for other administrators.
        /// </summary>
        Permissions = 1 << 4,

        /// <summary>
        ///     Ability to control the server like restart it or change the round type.
        /// </summary>
        Server = 1 << 5,

        /// <summary>
        ///     Ability to spawn stuff in.
        /// </summary>
        Spawn = 1 << 6,

        /// <summary>
        ///     Ability to use VV.
        /// </summary>
        VarEdit = 1 << 7,

        /// <summary>
        ///     Large mapping operations.
        /// </summary>
        Mapping = 1 << 8,

        /// <summary>
        ///     Makes you british.
        /// </summary>
        //Piss = 1 << 9,

        /// <summary>
        ///     Lets you view admin logs.
        /// </summary>
        Logs = 1 << 9,

        /// <summary>
        ///     Lets you modify the round (forcemap, loadgamemap, etc)
        /// </summary>
        Round = 1 << 10,

        /// <summary>
        ///     Lets you use BQL queries.
        /// </summary>
        Query = 1 << 11,

        /// <summary>
        ///     Lets you use the admin help system.
        /// </summary>
        Adminhelp = 1 << 12,

        /// <summary>
        ///     Lets you view admin notes.
        /// </summary>
        ViewNotes = 1 << 13,

        /// <summary>
        ///     Lets you create, edit and delete admin notes.
        /// </summary>
        EditNotes = 1 << 14,

        /// <summary>
        ///     Lets you Massban, on SS14.Admin
        /// </summary>
        MassBan = 1 << 15,

        /// <summary>
        /// Allows you to remain hidden from adminwho except to other admins with this flag.
        /// </summary>
        Stealth = 1 << 16,

        ///<summary>
		/// Allows you to use Admin chat
		///</summary>
		Adminchat = 1 << 17,

        ///<summary>
        /// Permits the visibility of Pii in game and on SS14 Admin
        ///</summary>
        Pii = 1 << 18,

        /// <summary>
        ///     Lets you take moderator actions on the game server.
        /// </summary>
        Moderator = 1 << 19,

        /// <summary>
        ///     Lets you check currently online admins.
        /// </summary>
        AdminWho = 1 << 20,

        /// <summary>
        ///     Lets you set the color of your OOC name.
        /// </summary>
        NameColor = 1 << 21,

        /// <summary>
        ///     Goobstation Full Admin extra perms.
        ///     Specifically used for Full Admin only.
        /// </summary>
        FullAdmin = 1 << 22,

        /// <summary>
        ///     Dangerous host permissions like scsi.
        /// </summary>
        Host = 1u << 31,
    }
}