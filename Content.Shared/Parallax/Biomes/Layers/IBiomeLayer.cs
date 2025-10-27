// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Noise;

namespace Content.Shared.Parallax.Biomes.Layers;

[ImplicitDataDefinitionForInheritors]
public partial interface IBiomeLayer
{
    /// <summary>
    /// Seed is used an offset from the relevant BiomeComponent's seed.
    /// </summary>
    FastNoiseLite Noise { get; }

    /// <summary>
    /// Threshold for this layer to be present. If set to 0 forces it for every tile.
    /// </summary>
    float Threshold { get; }

    /// <summary>
    /// Is the thresold inverted so we need to be lower than it.
    /// </summary>
    public bool Invert { get; }
}