using Content.Shared.Actions;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
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

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // movement speed
        SubscribeLocalEvent<SlaughterDemonComponent, RefreshMovementSpeedModifiersEvent>(RefreshMovement);

        // blood crawl
        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlExitEvent>(OnBloodCrawlExit);
        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlAttemptEvent>(OnBloodCrawlAttempt);

        // devouring
        SubscribeLocalEvent<SlaughterDemonComponent, SlaughterDevourEvent>(OnSlaughterDevour);

        // death related
        SubscribeLocalEvent<SlaughterDemonComponent, MobStateChangedEvent>(OnMobStateChanged);

        // polymorph shittery
        SubscribeLocalEvent<SlaughterDemonComponent, PolymorphedEvent>(OnPolymorph);

        // gun-related
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
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
        }
    }

    private void OnPolymorph(Entity<SlaughterDemonComponent> ent, ref PolymorphedEvent args)
    {
        if (!TryComp<SlaughterDevourComponent>(args.NewEntity, out var component))
            return;

        foreach (var entity in ent.Comp.ConsumedMobs)
        {
            if (entity == null)
                continue;

            _container.Insert(entity.Value, component.Container);
        }

        // Cooldown
        foreach (var action in _actions.GetActions(args.NewEntity))
            _actions.StartUseDelay(action.Owner);
    }

    private void OnBloodCrawlExit(Entity<SlaughterDemonComponent> ent, ref BloodCrawlExitEvent args)
    {
        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.NextUpdate;
        ent.Comp.ExitedBloodCrawl = true;
        _movementSpeedModifier.RefreshMovementSpeedModifiers(ent.Owner);

        SpawnAtPosition(ent.Comp.JauntUpEffect, Transform(ent.Owner).Coordinates);
    }

    private void OnSlaughterDevour(Entity<SlaughterDemonComponent> ent, ref SlaughterDevourEvent args)
    {
        var demonUid = ent.Owner;
        var demon = ent.Comp;
        var pullingEnt = args.pullingEnt;

        demon.ConsumedMobs.Add(pullingEnt);
        demon.Devoured++;

        if (!TryComp<SlaughterDevourComponent>(demonUid, out var slaughterDevour))
            return;

        _container.Insert(pullingEnt, slaughterDevour.Container);

        // Kill them for sure, just in case
        _mobState.ChangeMobState(pullingEnt, MobState.Dead);

        _audio.PlayPvs(slaughterDevour.FeastSound, args.PreviousCoordinates);

        _slaughterDevour.HealAfterDevouring(pullingEnt, demonUid, slaughterDevour);
        _slaughterDevour.IncrementObjective(demonUid,pullingEnt, demon);
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

    private void OnBloodCrawlAttempt(Entity<SlaughterDemonComponent> ent, ref BloodCrawlAttemptEvent args) =>
        SpawnAtPosition(ent.Comp.JauntEffect, Transform(ent.Owner).Coordinates);

    private void OnMobStateChanged(Entity<SlaughterDemonComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            _audio.PlayPvs(ent.Comp.DeathSound, ent.Owner, AudioParams.Default.WithVolume(-2f));
    }

    private void OnPickup(Entity<SlaughterDemonComponent> ent, ref PickupAttemptEvent args)
    {
        if (HasComp<GunComponent>(args.Item)
            && !ent.Comp.CanPickupGuns)
            args.Cancel();
    }
}
