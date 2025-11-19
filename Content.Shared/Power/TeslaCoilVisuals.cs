// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Power;

[Serializable, NetSerializable]
public enum TeslaCoilVisuals : byte
{
    Enabled,
    Lightning
}
