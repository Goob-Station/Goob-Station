// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Charges;

public sealed class ChargesChangedEvent : EntityEventArgs
{
    public readonly int CurrentCharges;
    public readonly int LastCharges;

    public ChargesChangedEvent(int current, int last)
    {
        CurrentCharges = current;
        LastCharges = last;
    }
}
