// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.DeviceNetwork.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.DeviceNetwork.Components;

/// <summary>
/// Allow entities to jam DeviceNetwork packets.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedDeviceNetworkJammerSystem))]
public sealed partial class DeviceNetworkJammerComponent : Component
{
    /// <summary>
    /// Range where packets will be jammed. This is checked both against the sender and receiver.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 5.0f;

    /// <summary>
    /// Device networks that can be jammed. For a list of default NetworkIds see DeviceNetIdDefaults on Content.Server.
    /// Network ids are not guaranteed to be limited to DeviceNetIdDefaults.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<string> JammableNetworks = [];

}