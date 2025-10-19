// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Parallax.Biomes.Layers;
using Robust.Shared.Noise; // Goob
using Robust.Shared.Prototypes;

namespace Content.Shared.Parallax.Biomes;

/// <summary>
/// A preset group of biome layers to be used for a <see cref="BiomeComponent"/>
/// </summary>
[Prototype]
public sealed partial class BiomeTemplatePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField("layers")]
    public List<IBiomeLayer> Layers = new();

    /// <summary>
    /// Goob - the last biome seed <see cref="LayerNoises"/> was cached for.
    /// </summary>
    public int? LastCachedSeed;

    /// <summary>
    /// Goob - the cached noises for each layer.
    /// Only valid for <see cref="LastCachedSeed"/>.
    /// </summary>
    public List<FastNoiseLite> LayerNoises = new();
}
