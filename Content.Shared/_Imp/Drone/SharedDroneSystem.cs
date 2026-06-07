// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Imp.Drone;

    public abstract class SharedDroneSystem : EntitySystem
    {
        [Serializable, NetSerializable]
        public enum DroneVisuals : byte
        {
            Status
        }

        [Serializable, NetSerializable]
        public enum DroneStatus : byte
        {
            Off,
            On
        }

        [Serializable, NetSerializable]
        public sealed class DroneBuiState : BoundUserInterfaceState
        {
            public float ChargePercent;

            public bool HasBattery;

            public DroneBuiState(float chargePercent, bool hasBattery)
            {
                ChargePercent = chargePercent;
                HasBattery = hasBattery;
            }
        }

        [Serializable, NetSerializable]
        public enum DroneUiKey : byte
        {
            Key
        }
    }
