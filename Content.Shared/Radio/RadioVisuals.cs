// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

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