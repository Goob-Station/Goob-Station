// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Flash;

[Serializable, NetSerializable]
public enum FlashVisuals : byte
{
    Burnt,
    Flashing,
}

[Serializable, NetSerializable]
public enum FlashVisualLayers : byte
{
    BaseLayer,
    LightLayer,
}
