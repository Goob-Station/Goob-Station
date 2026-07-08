// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._DV.VendingMachines;

[Serializable, NetSerializable]
public sealed class ShopVendorPurchaseMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}