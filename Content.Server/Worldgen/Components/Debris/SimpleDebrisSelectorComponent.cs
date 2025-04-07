// SPDX-FileCopyrightText: 2023 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Worldgen.Systems.Debris;
using Content.Server.Worldgen.Tools;
using Content.Shared.Storage;

namespace Content.Server.Worldgen.Components.Debris;

/// <summary>
///     This is used for a very simple debris selection for simple biomes. Just uses a spawn table.
/// </summary>
[RegisterComponent]
[Access(typeof(DebrisFeaturePlacerSystem))]
public sealed partial class SimpleDebrisSelectorComponent : Component
{
    private EntitySpawnCollectionCache? _cache;

    /// <summary>
    ///     The prototype-facing debris table entries.
    /// </summary>
    [DataField("debrisTable", required: true)]
    private List<EntitySpawnEntry> _entries = default!;

    /// <summary>
    ///     The debris entity spawn collection.
    /// </summary>
    public EntitySpawnCollectionCache CachedDebrisTable
    {
        get
        {
            _cache ??= new EntitySpawnCollectionCache(_entries);
            return _cache;
        }
    }
}
