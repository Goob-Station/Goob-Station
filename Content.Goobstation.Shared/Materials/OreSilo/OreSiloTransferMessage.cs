// SPDX-License-Identifier: AGPL-3.0-or-later
//Master Silo @Paul44ur
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Materials.OreSilo; //Works for now

[Serializable, NetSerializable]
public sealed class OreSiloTransferMessage(NetEntity target, string material, int sheets) : BoundUserInterfaceMessage
{
    public readonly NetEntity Target = target;

    public readonly string Material = material;

    public readonly int Sheets = sheets;
}
