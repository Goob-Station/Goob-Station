// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.Radio;

[Serializable, NetSerializable]
public enum RadioDeviceVisuals : byte
{
    Broadcasting,
    Speaker
}

[Serializable, NetSerializable]
public enum RadioDeviceVisualLayers : byte
{
    Broadcasting,
    Speaker
}
