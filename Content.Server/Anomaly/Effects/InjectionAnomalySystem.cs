// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Anomaly.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Anomaly.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using System.Linq;
using Robust.Server.GameObjects;

namespace Content.Server.Anomaly.Effects;
/// <summary>
/// This component allows the anomaly to inject liquid from the SolutionContainer
/// into the surrounding entities with the InjectionSolution component
/// </summary>
///

/// <see cref="InjectionAnomalyComponent"/>
public sealed class InjectionAnomalySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private EntityQuery<InjectableSolutionComponent> _injectableQuery;

    public override void Initialize()
    {
        SubscribeLocalEvent<InjectionAnomalyComponent, AnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<InjectionAnomalyComponent, AnomalySupercriticalEvent>(OnSupercritical, before: new[] { typeof(SharedSolutionContainerSystem) });

        _injectableQuery = GetEntityQuery<InjectableSolutionComponent>();
    }

    private void OnPulse(Entity<InjectionAnomalyComponent> entity, ref AnomalyPulseEvent args)
    {
        PulseScalableEffect(entity, entity.Comp.InjectRadius * args.PowerModifier, entity.Comp.MaxSolutionInjection * args.Severity * args.PowerModifier);
    }

    private void OnSupercritical(Entity<InjectionAnomalyComponent> entity, ref AnomalySupercriticalEvent args)
    {
        PulseScalableEffect(entity, entity.Comp.SuperCriticalInjectRadius * args.PowerModifier, entity.Comp.SuperCriticalSolutionInjection * args.PowerModifier);
    }

    private void PulseScalableEffect(Entity<InjectionAnomalyComponent> entity, float injectRadius, float maxInject)
    {
        if (!_solutionContainer.TryGetSolution(entity.Owner, entity.Comp.Solution, out _, out var sol))
            return;

        //We get all the entity in the radius into which the reagent will be injected.
        var xformQuery = GetEntityQuery<TransformComponent>();
        var xform = xformQuery.GetComponent(entity);
        var allEnts = _lookup.GetEntitiesInRange<InjectableSolutionComponent>(_transform.GetMapCoordinates(entity, xform: xform), injectRadius)
            .Select(x => x.Owner).ToList();

        //for each matching entity found
        foreach (var ent in allEnts)
        {
            if (!_solutionContainer.TryGetInjectableSolution(ent, out var injectable, out _))
                continue;

            if (_injectableQuery.TryGetComponent(ent, out var injEnt))
            {
                _solutionContainer.TryTransferSolution(injectable.Value, sol, maxInject);
                //Spawn Effect
                var uidXform = Transform(ent);
                Spawn(entity.Comp.VisualEffectPrototype, uidXform.Coordinates);
            }
        }
    }
}