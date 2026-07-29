// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;

namespace Content.Shared.Tiles; // Trauma - Moved to shared

/// <summary>
/// Applies effects upon stepping onto a tile.
/// </summary>
[RegisterComponent, Access(typeof(TileEntityEffectSystem))]
public sealed partial class TileEntityEffectComponent : Component
{
    /// <summary>
    /// List of effects that should be applied.
    /// </summary>
    [DataField]
    public List<EntityEffect> Effects = default!;
}
