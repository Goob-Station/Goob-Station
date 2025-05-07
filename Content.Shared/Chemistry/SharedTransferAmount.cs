// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry
{
    [Serializable, NetSerializable]
    public sealed class TransferAmountBoundInterfaceState : BoundUserInterfaceState
    {
        public FixedPoint2 Max;
        public FixedPoint2 Min;

        public TransferAmountBoundInterfaceState(FixedPoint2 max, FixedPoint2 min)
        {
            Max = max;
            Min = min;
        }
    }

    [Serializable, NetSerializable]
    public sealed class TransferAmountSetValueMessage : BoundUserInterfaceMessage
    {
        public FixedPoint2 Value;

        public TransferAmountSetValueMessage(FixedPoint2 value)
        {
            Value = value;
        }
    }

    [Serializable, NetSerializable]
    public enum TransferAmountUiKey
    {
        Key,
    }
}