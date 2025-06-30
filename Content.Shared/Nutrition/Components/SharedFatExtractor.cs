// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

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