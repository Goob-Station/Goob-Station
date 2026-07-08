// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.SurveillanceCamera;

[Serializable, NetSerializable]
public enum SurveillanceCameraVisualsKey : byte
{
    Key,
    Layer
}

[Serializable, NetSerializable]
public enum SurveillanceCameraVisuals : byte
{
    Active,
    InUse,
    Disabled,
    // Reserved for future use
    Xray,
    Emp
}