// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Spreader;

[Serializable, NetSerializable]
public enum KudzuVisuals : byte
{
    GrowthLevel,
    Variant
}
