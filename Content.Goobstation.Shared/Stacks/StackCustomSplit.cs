// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Stacks;

[Serializable, NetSerializable]
public sealed class StackCustomSplitAmountMessage : BoundUserInterfaceMessage
{
    public int Amount;

    public StackCustomSplitAmountMessage(int amount)
    {
        Amount = amount;
    }
}