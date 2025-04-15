// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.DeviceNetwork.Components
{
    [RegisterComponent]
    public sealed partial class StationLimitedNetworkComponent : Component
    {
        /// <summary>
        /// The station id the device is limited to.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public EntityUid? StationId;

        /// <summary>
        /// Whether the entity is allowed to receive packets from entities that are not tied to any station
        /// </summary>
        [DataField("allowNonStationPackets")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool AllowNonStationPackets = false;
    }
}