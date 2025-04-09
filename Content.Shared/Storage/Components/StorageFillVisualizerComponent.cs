// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Storage.Components;

/// <summary>
///     Change sprite depending on a storage fill percent.
/// </summary>
[RegisterComponent]
public sealed partial class StorageFillVisualizerComponent : Component
{
    [DataField("maxFillLevels", required: true)]
    public int MaxFillLevels;

    [DataField("fillBaseName", required: true)]
    public string FillBaseName = default!;
}

[Serializable, NetSerializable]
public enum StorageFillVisuals : byte
{
    FillLevel
}

[Serializable, NetSerializable]
public enum StorageFillLayers : byte
{
    Fill
}