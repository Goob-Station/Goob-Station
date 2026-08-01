// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Doors
{
    [Serializable, NetSerializable]
    public enum AirlockWireStatus
    {
        PowerIndicator,
        BoltIndicator,
        BoltLightIndicator,
        AiControlIndicator,
        AiVisionIndicator,
        TimingIndicator,
        SafetyIndicator,
    }
}