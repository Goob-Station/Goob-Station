using Content.Goobstation.Common.Devour;
using Content.Shared.Actions;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Polymorph;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.SlaughterDemon.Systems;

public abstract class SharedSlaughterDemonSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SlaughterDevourSystem _slaughterDevour = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    private EntityQuery<ActorComponent> _actorQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        // movement speed
        SubscribeLocalEvent<SlaughterDemonComponent, RefreshMovementSpeedModifiersEvent>(RefreshMovement);

        // blood crawl
        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlExitEvent>(OnBloodCrawlExit);
        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlAttemptEvent>(OnBloodCrawlAttempt);

        // devouring
        SubscribeLocalEvent<SlaughterDevourEvent>(OnSlaughterDevour);

        // polymorph shittery
        SubscribeLocalEvent<SlaughterDemonComponent, PolymorphedEvent>(OnPolymorph);

        // cant pickup items
        SubscribeLocalEvent<SlaughterDemonComponent, PickupAttemptEvent>(OnPickup);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlaughterDemonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.Accumulator || !comp.ExitedBloodCrawl)
                continue;

            comp.ExitedBloodCrawl = false;
            Dirty(uid, comp);
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
        }
    }

    private void OnPolymorph(Entity<SlaughterDemonComponent> ent, ref PolymorphedEvent args)
    {
        // Cooldown
        foreach (var action in _actions.GetActions(args.NewEntity))
        {
            _actions.StartUseDelay(action.Owner);
        }
    }

    private void OnBloodCrawlExit(Entity<SlaughterDemonComponent> ent, ref BloodCrawlExitEvent args)
    {
        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.NextUpdate;
        ent.Comp.ExitedBloodCrawl = true;
        Dirty(ent);

        _movementSpeedModifier.RefreshMovementSpeedModifiers(ent.Owner);

        PlayMeatySound(ent);
        PredictedSpawnAtPosition(ent.Comp.JauntUpEffect, Transform(ent.Owner).Coordinates);
    }

    private void OnSlaughterDevour(ref SlaughterDevourEvent args)
    {
        var uid = args.pullerEnt;
        var pullingEnt = args.pullingEnt;

        if (!TryComp<SlaughterDevourComponent>(uid, out var slaughterDevour)
            || slaughterDevour.Container == null)
            return;

        var evAttempt = new SlaughterDevourAttemptEvent(pullingEnt, uid);
        RaiseLocalEvent(pullingEnt, ref evAttempt);

        if (evAttempt.Cancelled)
            return;

        _container.Insert(pullingEnt, slaughterDevour.Container);

        // Stop them from being able to self-revive
        EnsureComp<PreventSelfRevivalComponent>(pullingEnt);

        // Kill them for sure, just in case
        if (_mobStateQuery.TryComp(pullingEnt, out var mobState))
            _mobState.ChangeMobState(pullingEnt, MobState.Dead, mobState);

        RemoveBlood(pullingEnt);

        _audio.PlayPredicted(slaughterDevour.FeastSound, Transform(uid).Coordinates, uid);

        _slaughterDevour.HealAfterDevouring(pullingEnt, uid, slaughterDevour);
        if (!TryComp(uid, out SlaughterDemonComponent? demon))
            return;

        _slaughterDevour.IncrementObjective(uid, pullingEnt, demon);
        demon.ConsumedMobs.Add(pullingEnt);
        demon.Devoured++;

        Dirty(uid, demon);

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
        if (args.Cancelled)
            return;

        PredictedSpawnAtPosition(ent.Comp.JauntEffect, Transform(ent.Owner).Coordinates);
    }

    private void OnPickup(Entity<SlaughterDemonComponent> ent, ref PickupAttemptEvent args) =>
        args.Cancel();

    protected virtual void RemoveBlood(EntityUid uid) {}

    #region Helper

    private void PlayMeatySound(Entity<SlaughterDemonComponent> ent)
    {
        if (_netManager.IsClient)
            return;

        if (!_random.Prob(ent.Comp.BloodCrawlSoundChance))
          return;

        var entities = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.BloodCrawlSoundLookup);
        foreach (var entity in entities)
        {
            if (entity == ent.Owner
                || !_actorQuery.HasComp(entity))
                continue;

            // ALEXA PLAY MEATY SOUND 🔊🔊
            _audio.PlayEntity(ent.Comp.BloodCrawlSounds, entity, ent.Owner);
        }
    }

    #endregion
}
