// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Binary.Components
{
    public sealed record GasVolumePumpData(float LastMolesTransferred);

    [Serializable, NetSerializable]
    public enum GasVolumePumpUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class GasVolumePumpToggleStatusMessage : BoundUserInterfaceMessage
    {
        public bool Enabled { get; }

        public GasVolumePumpToggleStatusMessage(bool enabled)
        {
            Enabled = enabled;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasVolumePumpChangeTransferRateMessage : BoundUserInterfaceMessage
    {
        public float TransferRate { get; }

        public GasVolumePumpChangeTransferRateMessage(float transferRate)
        {
            TransferRate = transferRate;
        }
    }
}