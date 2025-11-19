// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.Nutrition.Components;

[Serializable, NetSerializable]
public enum FatExtractorVisuals : byte
{
    Processing
}

public enum FatExtractorVisualLayers : byte
{
    Light,
    Stack,
    Smoke
}
