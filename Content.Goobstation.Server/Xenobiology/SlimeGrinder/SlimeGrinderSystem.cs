// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.Power.Components;
using Content.Shared.Audio;
using Content.Shared.Climbing.Events;
using Content.Shared.Construction.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Jittering;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Power;
using Content.Shared.Throwing;
using Robust.Server.Containers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Xenobiology.SlimeGrinder;

public sealed partial class SlimeGrinderSystem : EntitySystem
{
    [Dependency] private readonly XenobiologySystem _xenobio = default!;
    [Dependency] private readonly SharedJitteringSystem _jitteringSystem = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActiveSlimeGrinderComponent, ComponentInit>(OnActiveInit);
        SubscribeLocalEvent<ActiveSlimeGrinderComponent, ComponentShutdown>(OnActiveShutdown);
        SubscribeLocalEvent<ActiveSlimeGrinderComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);

        SubscribeLocalEvent<SlimeGrinderComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SlimeGrinderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlimeGrinderComponent, BeforeUnanchoredEvent>(OnUnanchored);
        SubscribeLocalEvent<SlimeGrinderComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<SlimeGrinderComponent, ClimbedOnEvent>(OnClimbedOn);
        SubscribeLocalEvent<SlimeGrinderComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<SlimeGrinderComponent, ReclaimerDoAfterEvent>(OnDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveSlimeGrinderComponent, SlimeGrinderComponent>();
        while (query.MoveNext(out var uid, out _, out var grinder))
        {
            grinder.ProcessingTimer -= frameTime;

            if (grinder.ProcessingTimer > 0
                || !TryComp<SlimeComponent>(grinder.EntityGrinded, out var slime))
                continue;

            var extractProto = _xenobio.GetProducedExtract((grinder.EntityGrinded.Value, slime));
            SpawnNextToOrDrop(extractProto, uid);

            QueueDel(grinder.EntityGrinded);
            grinder.EntityGrinded = null;

            RemCompDeferred<ActiveSlimeGrinderComponent>(uid);
        }
    }

    #region  Active Grinding

    private void OnActiveInit(Entity<ActiveSlimeGrinderComponent> activeGrinder, ref ComponentInit args)
    {
        if (!TryComp<SlimeGrinderComponent>(activeGrinder, out var grinder))
            return;

        _jitteringSystem.AddJitter(activeGrinder, -10, 100);
        _sharedAudioSystem.PlayPvs(grinder.GrindSound, activeGrinder);
        _ambientSoundSystem.SetAmbience(activeGrinder, true);
    }

    private void OnActiveShutdown(Entity<ActiveSlimeGrinderComponent> activeGrinder, ref ComponentShutdown args)
    {
        RemComp<JitteringComponent>(activeGrinder);
        _ambientSoundSystem.SetAmbience(activeGrinder, false);
    }

    private void OnUnanchorAttempt(Entity<ActiveSlimeGrinderComponent> activeGrinder, ref UnanchorAttemptEvent args) =>
        args.Cancel();

    private void OnPowerChanged(EntityUid uid, SlimeGrinderComponent component, ref PowerChangedEvent args)
    {
        if (args.Powered)
        {
            if (component.ProcessingTimer > 0)
                EnsureComp<ActiveSlimeGrinderComponent>(uid);
        }
        else
            RemComp<ActiveSlimeGrinderComponent>(uid);
    }

    #endregion

    private void OnInit(Entity<SlimeGrinderComponent> grinder, ref MapInitEvent args) =>
        grinder.Comp.GrindedContainer = _container.EnsureContainer<Container>(grinder, "GrindedContainer");

    private void OnShutdown(Entity<SlimeGrinderComponent> grinder, ref ComponentShutdown args) =>
        _container.EmptyContainer(grinder.Comp.GrindedContainer);
    private void OnUnanchored(Entity<SlimeGrinderComponent> grinder, ref BeforeUnanchoredEvent args) =>
        _container.EmptyContainer(grinder.Comp.GrindedContainer);
    private void OnAfterInteractUsing(Entity<SlimeGrinderComponent> grinder, ref AfterInteractUsingEvent args)
    {
        if (!args.CanReach
            || args.Target == null
            || !TryComp<PhysicsComponent>(args.Used, out var physics)
            || !CanGrind(grinder, args.Used))
            return;

        var delay = grinder.Comp.BaseInsertionDelay * physics.FixturesMass;
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, delay, new ReclaimerDoAfterEvent(), grinder, target: args.Target, used: args.Used)
        {
            NeedHand = true,
            BreakOnMove = true,
        });
    }

    private void OnClimbedOn(Entity<SlimeGrinderComponent> reclaimer, ref ClimbedOnEvent args)
    {
        if (!CanGrind(reclaimer, args.Climber))
        {
            var direction = new Vector2(_robustRandom.Next(-2, 2), _robustRandom.Next(-2, 2));
            _throwing.TryThrow(args.Climber, direction, 0.5f);
            return;
        }

        StartProcessing(args.Climber, reclaimer);
    }

    private void OnDoAfter(Entity<SlimeGrinderComponent> reclaimer, ref ReclaimerDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Args.Used is not { } toProcess)
            return;

        StartProcessing(toProcess, reclaimer);
        args.Handled = true;
    }

    private void StartProcessing(EntityUid toProcess, Entity<SlimeGrinderComponent> grinder, PhysicsComponent? physics = null, SlimeComponent? slime = null)
    {
        if (!Resolve(toProcess, ref physics, ref slime))
            return;

        EnsureComp<ActiveSlimeGrinderComponent>(grinder);
        grinder.Comp.ProcessingTimer = physics.FixturesMass * grinder.Comp.ProcessingTimePerUnitMass;
        grinder.Comp.EntityGrinded = toProcess;

        _container.EmptyContainer(slime.Stomach);
        _container.Insert(toProcess, grinder.Comp.GrindedContainer);
    }

    private bool CanGrind(Entity<SlimeGrinderComponent> grinder, EntityUid dragged)
    {
        if (HasComp<ActiveSlimeGrinderComponent>(grinder)
            || !Transform(grinder).Anchored
            || !TryComp<MobStateComponent>(dragged, out var mobState)
            || mobState.CurrentState != MobState.Dead)
            return false;

        return !TryComp<ApcPowerReceiverComponent>(grinder, out var power) || power.Powered;
    }


}
