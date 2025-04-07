// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.LinkAccount;

[Serializable, NetSerializable]
public sealed class SharedRMCPatron(string name, string tier)
{
    public readonly string Name = name;
    public readonly string Tier = tier;
}