// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.LinkAccount;

[Serializable, NetSerializable]
public sealed class SharedRMCPatron(string name, string tier)
{
    public readonly string Name = name;
    public readonly string Tier = tier;
}