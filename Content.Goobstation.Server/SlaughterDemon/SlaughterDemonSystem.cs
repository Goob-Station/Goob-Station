// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Goobstation.Shared.SlaughterDemon.Objectives;
using Content.Server.Mind;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Fluids.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SlaughterDemon;

public sealed class SlaughterDemonSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SlaughterDevourSystem _slaughterDevour = default!;

    private EntityQuery<PuddleComponent> _puddleQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _puddleQuery = GetEntityQuery<PuddleComponent>();

        SubscribeLocalEvent<SlaughterDemonComponent, RefreshMovementSpeedModifiersEvent>(RefreshMovement);

        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlExitEvent>(OnBloodCrawlExit);
        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlAttemptEvent>(OnBloodCrawlAttempt);

        SubscribeLocalEvent<SlaughterDemonComponent, SlaughterDevourEvent>(OnSlaughterDevour);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlaughterDemonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);

            if (_timing.CurTime >= comp.Accumulator)
                comp.ExitedBloodCrawl = false;
        }
    }

    private void OnBloodCrawlExit(Entity<SlaughterDemonComponent> ent, ref BloodCrawlExitEvent args)
    {
        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.NextUpdate;
        ent.Comp.ExitedBloodCrawl = true;

        SpawnAtPosition(ent.Comp.JauntUpEffect, Transform(ent.Owner).Coordinates);
    }

    private void OnSlaughterDevour(Entity<SlaughterDemonComponent> ent, ref SlaughterDevourEvent args)
    {
        var demonUid = ent.Owner;
        var demon = ent.Comp;
        var pullingEnt = args.pullingEnt;

        demon.ConsumedMobs.Add(pullingEnt);
        demon.Devoured++;

        if (TryComp<SlaughterDevourComponent>(demonUid, out var slaughterDevour))
            _slaughterDevour.HealAfterDevouring(pullingEnt, demonUid, slaughterDevour);

        _slaughterDevour.IncrementObjective(demonUid,pullingEnt, demon);
        QueueDel(pullingEnt);
    }

    private void RefreshMovement(EntityUid uid,
        SlaughterDemonComponent component,
        RefreshMovementSpeedModifiersEvent args)
    {
        if (component.ExitedBloodCrawl)
        {
            args.ModifySpeed(component.SpeedModWalk, component.SpeedModRun);
        }
        else
        {
            args.ModifySpeed(1f, 1f);
        }
    }

    private void OnBloodCrawlAttempt(Entity<SlaughterDemonComponent> ent, ref BloodCrawlAttemptEvent args)
    {
        SpawnAtPosition(ent.Comp.JauntEffect, Transform(ent.Owner).Coordinates);
    }

    # region Helper Functions

    /// <summary>
    /// Detects if an entity is standing on blood, or not.
    /// </summary>
    public bool IsStandingOnBlood(Entity<BloodCrawlComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return false;

        var ents = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRange);
        foreach (var entity in ents)
        {
            if (!_puddleQuery.TryComp(entity, out var puddle))
            {
                Logger.Warning($"Failed to resolve component for puddle {ToPrettyString(entity)}");
                continue;
            }

            if (!_solutionContainerSystem.ResolveSolution(entity, puddle.SolutionName, ref puddle.Solution, out var solution))
            {
                Logger.Info($"Resolving solution failed");
                continue;
            }

            foreach (var reagent in solution.Contents)
            {
                if (reagent.Reagent.Prototype == ent.Comp.Blood)
                    return true;
            }
        }
        return false;
    }

    # endregion
}
