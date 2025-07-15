// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Server.Body.Components;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SlaughterDemon;

public sealed class SlaughterDemonSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SlaughterDevourSystem _slaughterDevour = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlaughterDemonComponent, RefreshMovementSpeedModifiersEvent>(RefreshMovement);

        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlExitEvent>(OnBloodCrawlExit);
        SubscribeLocalEvent<SlaughterDemonComponent, BloodCrawlAttemptEvent>(OnBloodCrawlAttempt);

        SubscribeLocalEvent<SlaughterDemonComponent, SlaughterDevourEvent>(OnSlaughterDevour);

        SubscribeLocalEvent<SlaughterDemonComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<SlaughterDemonComponent, BeingGibbedEvent>(OnGib);
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

        if (!TryComp<SlaughterDevourComponent>(demonUid, out var slaughterDevour))
            return;
        // todo: find way in the future to make the feast sound work (i hate polymorphs)
        if (ent.Comp.IsLaughter)
        {
            _container.Insert(pullingEnt, slaughterDevour.Container);
        }
        else
        {
            QueueDel(pullingEnt);
        }

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

    private void OnBloodCrawlAttempt(Entity<SlaughterDemonComponent> ent, ref BloodCrawlAttemptEvent args)
    {
        SpawnAtPosition(ent.Comp.JauntEffect, Transform(ent.Owner).Coordinates);
    }

    private void OnMobStateChanged(Entity<SlaughterDemonComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            _audio.PlayPvs(ent.Comp.DeathSound, ent.Owner, AudioParams.Default.WithVolume(-2f));
    }

    private void OnGib(Entity<SlaughterDemonComponent> ent, ref BeingGibbedEvent args)
    {
        if (ent.Comp.IsLaughter)
        {
            if (TryComp<SlaughterDevourComponent>(ent.Owner, out var devour))
                _container.EmptyContainer(devour.Container);
        }
    }
}
