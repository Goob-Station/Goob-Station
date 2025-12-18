// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Lavaland.Biome;

/// <summary>
/// Optimization component that stops the planet from unloading chunks, or loading chunks that aren't in the area.
/// Add this to planet maps that have known borders to reduce lag.
/// </summary>
/// <remarks>
/// This will just make your server's RAM suffer more in exchange for CPU.
/// </remarks>
[RegisterComponent]
public sealed partial class BiomeOptimizeComponent : Component
{
    [DataField]
    public Box2 LoadArea;

    [ViewVariables]
    public HashSet<Vector2i> LoadedChunks = new();
}
