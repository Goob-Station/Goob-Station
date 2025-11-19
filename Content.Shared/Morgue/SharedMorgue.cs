// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Morgue;

[Serializable, NetSerializable]
public enum MorgueVisuals : byte
{
    Contents
}

[Serializable, NetSerializable]
public enum MorgueContents : byte
{
    Empty,
    HasMob,
    HasSoul,
    HasContents,
}

[Serializable, NetSerializable]
public enum CrematoriumVisuals : byte
{
    Burning,
}
