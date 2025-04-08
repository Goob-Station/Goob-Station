// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Michael Phillips <1194692+MeltedPixel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Morbo <exstrominer@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Chat
{
    /// <summary>
    ///     Chat channels that the player can select in the chat box.
    /// </summary>
    /// <remarks>
    ///     Maps to <see cref="ChatChannel"/>, giving better names.
    /// </remarks>
    [Flags]
    public enum ChatSelectChannel : uint // Goobstation - Starlight collective mind port
    {
        None = 0,

        /// <summary>
        ///     Chat heard by players within earshot
        /// </summary>
        Local = ChatChannel.Local,

        /// <summary>
        ///     Chat heard by players right next to each other
        /// </summary>
        Whisper = ChatChannel.Whisper,

        /// <summary>
        ///     Radio messages
        /// </summary>
        Radio = ChatChannel.Radio,

        /// <summary>
        ///     Local out-of-character channel
        /// </summary>
        LOOC = ChatChannel.LOOC,

        /// <summary>
        ///     Out-of-character channel
        /// </summary>
        OOC = ChatChannel.OOC,

        /// <summary>
        ///     Emotes
        /// </summary>
        Emotes = ChatChannel.Emotes,

        // Goobstation - Starlight collective mind port
        /// <summary>
        ///     CollectiveMind
        /// </summary>
        CollectiveMind = ChatChannel.CollectiveMind,

        /// <summary>
        ///     Deadchat
        /// </summary>
        Dead = ChatChannel.Dead,

        /// <summary>
        ///     Admin chat
        /// </summary>
        Admin = ChatChannel.AdminChat,

        /// <summary>
        ///     Nyano - Summary:. Telepathic channel for all psionic entities.
        /// </summary>
        Telepathic = ChatChannel.Telepathic,

        Console = ChatChannel.Unspecified
    }
}