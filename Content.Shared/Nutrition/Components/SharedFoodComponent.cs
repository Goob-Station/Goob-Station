// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Nutrition.Components
{
    // TODO: Remove maybe? Add visualizer for food
    [Serializable, NetSerializable]
    public enum FoodVisuals : byte
    {
        Visual,
        MaxUses,
    }

    [Serializable, NetSerializable]
    public enum OpenableVisuals : byte
    {
        Opened,
        Layer
    }

    [Serializable, NetSerializable]
    public enum SealableVisuals : byte
    {
        Sealed,
        Layer,
    }
}
