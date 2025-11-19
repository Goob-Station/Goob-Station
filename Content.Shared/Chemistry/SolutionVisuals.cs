// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry
{
    [Serializable, NetSerializable]
    public enum SolutionContainerVisuals : byte
    {
        Color,
        FillFraction,
        BaseOverride,
        SolutionName
    }

    public enum SolutionContainerLayers : byte
    {
        Fill,
        Base,
        Overlay
    }
}
