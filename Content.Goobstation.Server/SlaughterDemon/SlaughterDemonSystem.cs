using Content.Goobstation.Server.DragDrop;
using Content.Goobstation.Shared.DragDrop;
using Content.Goobstation.Shared.SlaughterDemon;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Interaction;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DragDrop;
using Content.Shared.Fluids.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.StepTrigger.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SlaughterDemon;

public sealed class SlaughterDemonSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly PuddleSystem _puddleSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly TileSystem _tileSystem = default!;
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

        SubscribeLocalEvent<SlaughterDemonComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<SlaughterDemonComponent, RefreshMovementSpeedModifiersEvent>(RefreshMovement);
        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlAttemptEvent>(OnBloodCrawlAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlaughterDemonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.Accumulator)
                continue;

            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            comp.Accumulator = _timing.CurTime + comp.NextUpdate;

            if (!IsStandingOnBlood(uid))
            {
                comp.IsOnBlood = false;
                continue;
            }

            comp.IsOnBlood = true;
        }
    }

    private void OnStartup(Entity<SlaughterDemonComponent> ent, ref ComponentStartup startup)
    {
        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.NextUpdate;
    }

    private void RefreshMovement(EntityUid uid,
        SlaughterDemonComponent component,
        RefreshMovementSpeedModifiersEvent args)
    {
        if (component.IsOnBlood)
        {
            args.ModifySpeed(component.speedModWalk, component.speedModRun);
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

    // Exclusive to slaughter demons. They devour targets once they enter blood crawl jaunt form.
    // Laughter demons do not directly devour them, however.
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

    public bool IsStandingOnBlood(Entity<BloodCrawlComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return false;

        // Before you ask "why lookup"
        // the answer is footprints goida
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
