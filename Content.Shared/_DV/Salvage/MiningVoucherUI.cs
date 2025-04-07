// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Salvage;

/// <summary>
/// Message for a mining voucher kit to be selected.
/// </summary>
[Serializable, NetSerializable]
public sealed class MiningVoucherSelectMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}

[Serializable, NetSerializable]
public enum MiningVoucherUiKey : byte
{
    Key
}