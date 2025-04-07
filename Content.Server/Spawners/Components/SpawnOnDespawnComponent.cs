// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Spawners.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server.Spawners.Components;

/// <summary>
/// When a <c>TimedDespawnComponent"</c> despawns, another one will be spawned in its place.
/// </summary>
[RegisterComponent, Access(typeof(SpawnOnDespawnSystem))]
public sealed partial class SpawnOnDespawnComponent : Component
{
    /// <summary>
    /// Entity prototype to spawn.
    /// </summary>
    [DataField]
    public EntProtoId? Prototype; // Lavaland Change

    // Lavaland Change start - make it spawn more. without specific intrusion.
    [DataField]
    public List<EntProtoId> Prototypes = [];
    // Lavaland Change end
}