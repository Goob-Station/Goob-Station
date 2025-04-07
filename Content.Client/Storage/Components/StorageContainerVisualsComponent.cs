// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Chemistry.Components;

namespace Content.Client.Storage.Components;

/// <summary>
///     Essentially a version of <see cref="SolutionContainerVisualsComponent"/> fill level handling but for item storage.
///     Depending on the fraction of storage that's filled, will change the sprite at <see cref="FillLayer"/> to the nearest
///     fill level, up to <see cref="MaxFillLevels"/>.
/// </summary>
[RegisterComponent]
public sealed partial class StorageContainerVisualsComponent : Component
{
    [DataField("maxFillLevels")]
    public int MaxFillLevels = 0;

    /// <summary>
    ///     A prefix to use for the fill states., i.e. {FillBaseName}{fill level} for the state
    /// </summary>
    [DataField("fillBaseName")]
    public string? FillBaseName;

    [DataField("layer")]
    public StorageContainerVisualLayers FillLayer = StorageContainerVisualLayers.Fill;
}

public enum StorageContainerVisualLayers : byte
{
    Fill
}