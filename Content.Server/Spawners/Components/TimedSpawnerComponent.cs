// SPDX-FileCopyrightText: 2020 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server.Spawners.Components;

/// <summary>
/// Spawns entities at a set interval.
/// Can configure the set of entities, spawn timing, spawn chance,
/// and min/max number of entities to spawn.
/// </summary>
[RegisterComponent, EntityCategory("Spawner")]
public sealed partial class TimedSpawnerComponent : Component, ISerializationHooks
{
    /// <summary>
    /// List of entities that can be spawned by this component. One will be randomly
    /// chosen for each entity spawned. When multiple entities are spawned at once,
    /// each will be randomly chosen separately.
    /// </summary>
    [DataField]
    public List<EntProtoId> Prototypes = [];

    /// <summary>
    /// Chance of an entity being spawned at the end of each interval.
    /// </summary>
    [DataField]
    public float Chance = 1.0f;

    /// <summary>
    /// Length of the interval between spawn attempts.
    /// </summary>
    [DataField]
    public int IntervalSeconds = 60;

    /// <summary>
    /// The minimum number of entities that can be spawned when an interval elapses.
    /// </summary>
    [DataField]
    public int MinimumEntitiesSpawned = 1;

    /// <summary>
    /// The maximum number of entities that can be spawned when an interval elapses.
    /// </summary>
    [DataField]
    public int MaximumEntitiesSpawned = 1;

    public CancellationTokenSource? TokenSource;

    void ISerializationHooks.AfterDeserialization()
    {
        if (MinimumEntitiesSpawned > MaximumEntitiesSpawned)
            throw new ArgumentException("MaximumEntitiesSpawned can't be lower than MinimumEntitiesSpawned!");
    }
}