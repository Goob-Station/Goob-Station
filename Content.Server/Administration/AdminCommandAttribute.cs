// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration;
using JetBrains.Annotations;

namespace Content.Server.Administration
{
    /// <summary>
    ///     Specifies that a command can only be executed by an admin with the specified flags.
    /// </summary>
    /// <remarks>
    ///     If this attribute is used multiple times, either attribute's flag sets can be used to get access.
    /// </remarks>
    /// <seealso cref="AnyCommandAttribute"/>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [MeansImplicitUse]
    public sealed class AdminCommandAttribute : Attribute
    {
        public AdminCommandAttribute(AdminFlags flags)
        {
            Flags = flags;
        }

        public AdminFlags Flags { get; }
    }
}