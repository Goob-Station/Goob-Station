// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Power
{
    [Serializable, NetSerializable]
    public enum PowerDeviceVisuals : byte
    {
        VisualState,
        Powered,
        BatteryPowered
    }
}