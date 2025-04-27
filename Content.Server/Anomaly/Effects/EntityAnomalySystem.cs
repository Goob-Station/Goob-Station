// SPDX-FileCopyrightText: 2023 KISS <59531932+YuriyKiss@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Yurii Kis <yurii.kis@smartteksas.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Anomaly;
using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Effects;
using Content.Shared.Anomaly.Effects.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Server.Anomaly.Effects;

public sealed class EntityAnomalySystem : SharedEntityAnomalySystem
{
    [Dependency] private readonly SharedAnomalySystem _anomaly = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<EntitySpawnAnomalyComponent, AnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<EntitySpawnAnomalyComponent, AnomalySupercriticalEvent>(OnSupercritical);
        SubscribeLocalEvent<EntitySpawnAnomalyComponent, AnomalyStabilityChangedEvent>(OnStabilityChanged);
        SubscribeLocalEvent<EntitySpawnAnomalyComponent, AnomalySeverityChangedEvent>(OnSeverityChanged);
        SubscribeLocalEvent<EntitySpawnAnomalyComponent, AnomalyShutdownEvent>(OnShutdown);
    }

    private void OnPulse(Entity<EntitySpawnAnomalyComponent> component, ref AnomalyPulseEvent args)
    {
        foreach (var entry in component.Comp.Entries)
        {
            if (!entry.Settings.SpawnOnPulse)
                continue;

            SpawnEntities(component, entry, args.Stability, args.Severity, args.PowerModifier);
        }
    }

    private void OnSupercritical(Entity<EntitySpawnAnomalyComponent> component, ref AnomalySupercriticalEvent args)
    {
        foreach (var entry in component.Comp.Entries)
        {
            if (!entry.Settings.SpawnOnSuperCritical)
                continue;

            SpawnEntities(component, entry, 1, 1, args.PowerModifier);
        }
    }

    private void OnShutdown(Entity<EntitySpawnAnomalyComponent> component, ref AnomalyShutdownEvent args)
    {
        foreach (var entry in component.Comp.Entries)
        {
            if (!entry.Settings.SpawnOnShutdown || args.Supercritical)
                continue;

            SpawnEntities(component, entry, 1, 1, 1);
        }
    }

    private void OnStabilityChanged(Entity<EntitySpawnAnomalyComponent> component, ref AnomalyStabilityChangedEvent args)
    {
        foreach (var entry in component.Comp.Entries)
        {
            if (!entry.Settings.SpawnOnStabilityChanged)
                continue;

            SpawnEntities(component, entry, args.Stability, args.Severity, 1);
        }
    }

    private void OnSeverityChanged(Entity<EntitySpawnAnomalyComponent> component, ref AnomalySeverityChangedEvent args)
    {
        foreach (var entry in component.Comp.Entries)
        {
            if (!entry.Settings.SpawnOnSeverityChanged)
                continue;

            SpawnEntities(component, entry, args.Stability, args.Severity, 1);
        }
    }

    private void SpawnEntities(Entity<EntitySpawnAnomalyComponent> anomaly, EntitySpawnSettingsEntry entry, float stability, float severity, float powerMod)
    {
        var xform = Transform(anomaly);
        if (!TryComp(xform.GridUid, out MapGridComponent? grid))
            return;

        var tiles = _anomaly.GetSpawningPoints(anomaly, stability, severity, entry.Settings, powerMod);
        if (tiles == null)
            return;

        foreach (var tileref in tiles)
        {
            Spawn(_random.Pick(entry.Spawns), _mapSystem.ToCenterCoordinates(tileref, grid));
        }
    }
}