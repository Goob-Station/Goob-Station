// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class DockingInterfaceState
{
    public Dictionary<NetEntity, List<DockingPortState>> Docks;

    public DockingInterfaceState(Dictionary<NetEntity, List<DockingPortState>> docks)
    {
        Docks = docks;
    }
}
