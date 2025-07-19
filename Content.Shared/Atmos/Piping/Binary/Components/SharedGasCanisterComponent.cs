// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Justin Trotter <trotter.justin@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Binary.Components
{
    /// <summary>
    /// Key representing which <see cref="PlayerBoundUserInterface"/> is currently open.
    /// Useful when there are multiple UI for an object. Here it's future-proofing only.
    /// </summary>
    [Serializable, NetSerializable]
    public enum GasCanisterUiKey : byte
    {
        Key,
    }

    #region Enums

    /// <summary>
    /// Used in <see cref="GasCanisterVisualizer"/> to determine which visuals to update.
    /// </summary>
    [Serializable, NetSerializable]
    public enum GasCanisterVisuals
    {
        PressureState,
        TankInserted
    }

    #endregion

    /// <summary>
    /// Represents a <see cref="GasCanisterComponent"/> state that can be sent to the client
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GasCanisterBoundUserInterfaceState : BoundUserInterfaceState
    {
        public float CanisterPressure { get; }
        public bool PortStatus { get; }
        public float TankPressure { get; }

        public GasCanisterBoundUserInterfaceState(float canisterPressure, bool portStatus, float tankPressure)
        {
            CanisterPressure = canisterPressure;
            PortStatus = portStatus;
            TankPressure = tankPressure;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasCanisterHoldingTankEjectMessage : BoundUserInterfaceMessage
    {
        public GasCanisterHoldingTankEjectMessage()
        {}
    }

    [Serializable, NetSerializable]
    public sealed class GasCanisterChangeReleasePressureMessage : BoundUserInterfaceMessage
    {
        public float Pressure { get; }

        public GasCanisterChangeReleasePressureMessage(float pressure)
        {
            Pressure = pressure;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasCanisterChangeReleaseValveMessage : BoundUserInterfaceMessage
    {
        public bool Valve { get; }

        public GasCanisterChangeReleaseValveMessage(bool valve)
        {
            Valve = valve;
        }
    }
}