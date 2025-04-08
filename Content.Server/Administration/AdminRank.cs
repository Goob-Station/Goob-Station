// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration;


namespace Content.Server.Administration
{
    public sealed class AdminRank
    {
        public AdminRank(string name, AdminFlags flags)
        {
            Name = name;
            Flags = flags;
        }

        public string Name { get; }
        public AdminFlags Flags { get; }
    }
}