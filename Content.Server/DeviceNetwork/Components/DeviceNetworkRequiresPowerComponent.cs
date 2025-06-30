// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.DeviceNetwork.Components;

/// <summary>
///     Component that indicates that this device networked entity requires power
///     in order to receive a packet. Having this component will cancel all packet events
///     if the entity is not powered.
/// </summary>
[RegisterComponent]
public sealed partial class DeviceNetworkRequiresPowerComponent : Component
{
}