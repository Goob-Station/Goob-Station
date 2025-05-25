// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Procedural.Distance;

namespace Content.Shared.Procedural.DungeonGenerators;

/// <summary>
/// Like <see cref="Content.Shared.Procedural.DungeonGenerators.NoiseDunGenLayer"/> except with maximum dimensions
/// </summary>
public sealed partial class NoiseDistanceDunGen : IDunGenLayer
{
    [DataField]
    public IDunGenDistance? DistanceConfig;

    [DataField]
    public Vector2i Size;

    [DataField(required: true)]
    public List<NoiseDunGenLayer> Layers = new();
}