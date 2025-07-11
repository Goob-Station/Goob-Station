// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids.Components;
using Content.Shared.Humanoid;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SlaughterDemon;

public sealed class SlaughterDemonSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<PullerComponent> _pullerQuery;
    private EntityQuery<PuddleComponent> _puddleQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _pullerQuery = GetEntityQuery<PullerComponent>();
        _puddleQuery = GetEntityQuery<PuddleComponent>();

        SubscribeLocalEvent<SlaughterDemonComponent, RefreshMovementSpeedModifiersEvent>(RefreshMovement);

        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlExitEvent>(OnBloodCrawlExit);
        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlAttemptEvent>(OnBloodCrawlAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlaughterDemonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.Accumulator)
            {
                comp.ExitedBloodCrawl = false;
                continue;
            }

            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
        }
    }

    private void OnBloodCrawlExit(Entity<SlaughterDemonComponent> ent, ref BloodCrawlExitEvent args)
    {
        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.NextUpdate;
        ent.Comp.ExitedBloodCrawl = true;
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
        TryDevour(ent.Owner);
    }

    /// <summary>
    /// Exclusive to slaughter demons. They devour targets once they enter blood crawl jaunt form.
    /// Laughter demons do not directly devour them, however.
    /// </summary>
    private void TryDevour(EntityUid uid)
    {
        if (!TryComp<SlaughterDemonComponent>(uid, out var demon))
            return;

        if (!_pullerQuery.TryComp(uid, out var puller))
            return;

        if (!HasComp<HumanoidAppearanceComponent>(puller.Pulling))
            return;

        demon.ConsumedMobs.Add(puller.Pulling.Value);
        demon.Devoured++;
        QueueDel(puller.Pulling);
        Logger.Info("Entity {puller.Pulling.Value} devoured by entity: {uid}");
    }

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
}
