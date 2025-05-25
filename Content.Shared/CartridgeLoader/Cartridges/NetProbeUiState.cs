// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NetProbeUiState : BoundUserInterfaceState
{
    /// <summary>
    /// The list of probed network devices
    /// </summary>
    public List<ProbedNetworkDevice> ProbedDevices;

    public NetProbeUiState(List<ProbedNetworkDevice> probedDevices)
    {
        ProbedDevices = probedDevices;
    }
}

[Serializable, NetSerializable, DataRecord]
public sealed partial class ProbedNetworkDevice
{
    public readonly string Name;
    public readonly string Address;
    public readonly string Frequency;
    public readonly string NetId;

    public ProbedNetworkDevice(string name, string address, string frequency, string netId)
    {
        Name = name;
        Address = address;
        Frequency = frequency;
        NetId = netId;
    }
}