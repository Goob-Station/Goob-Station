// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Condemned;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.HellGoose.Components;
using Content.Goobstation.Shared.Maps;
using Content.Server._Shitmed.StatusEffects;
using Content.Server.IdentityManagement;
using Content.Server.Polymorph.Systems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.EntitySerialization;

namespace Content.Goobstation.Server.Devil.Condemned;

public sealed partial class CondemnedSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PolymorphSystem _poly = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ScrambleDnaEffectSystem _scramble = default!;
    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CondemnedComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CondemnedComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<CondemnedComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<CondemnedComponent, UpdateCanMoveEvent>(OnMoveAttempt);
        InitializeOnDeath();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CondemnedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            switch (comp.CurrentPhase)
            {
                case CondemnedPhase.PentagramActive:
                    UpdatePentagramPhase(uid, frameTime, comp);
                    break;
                case CondemnedPhase.HandActive:
                    UpdateHandPhase(uid, frameTime, comp);
                    break;
            }
        }
    }

    private void OnStartup(Entity<CondemnedComponent> condemned, ref MapInitEvent args)
    {
        if (condemned.Comp.SoulOwnedNotDevil)
            return;

        if (HasComp<WeakToHolyComponent>(condemned))
            condemned.Comp.WasWeakToHoly = true;
        else
            EnsureComp<WeakToHolyComponent>(condemned).AlwaysTakeHoly = true;
    }

    private void OnRemoved(Entity<CondemnedComponent> condemned, ref ComponentRemove args)
    {
        if (condemned.Comp.SoulOwnedNotDevil)
            return;

        if (!condemned.Comp.WasWeakToHoly)
            RemComp<WeakToHolyComponent>(condemned);
    }

    private void OnMoveAttempt(Entity<CondemnedComponent> condemned, ref UpdateCanMoveEvent args)
    {
        if (!condemned.Comp.FreezeDuringCondemnation
            || condemned.Comp.CurrentPhase != CondemnedPhase.Waiting)
            return;

        args.Cancel();
    }

    public void StartCondemnation(
        EntityUid uid,
        bool freezeEntity = true,
        bool doFlavor = true,
        CondemnedBehavior behavior = CondemnedBehavior.Delete)
    {
        var comp = EnsureComp<CondemnedComponent>(uid);
        comp.CondemnOnDeath = false;

        if (freezeEntity)
            comp.FreezeDuringCondemnation = true;

        var coords = Transform(uid).Coordinates;
        Spawn(comp.PentagramProto, coords);
        _audio.PlayPvs(comp.SoundEffect, coords);

        if (comp.CondemnedBehavior == CondemnedBehavior.Delete && doFlavor)
            _popup.PopupCoordinates(Loc.GetString("condemned-start", ("target", uid)), coords, PopupType.LargeCaution);

        comp.CurrentPhase = CondemnedPhase.PentagramActive;
        comp.PhaseTimer = 0f;
        comp.CondemnedBehavior = behavior;
    }

    private void UpdatePentagramPhase(EntityUid uid, float frameTime, CondemnedComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.PhaseTimer += frameTime;

        if (comp.PhaseTimer < 3f)
            return;

        var coords = Transform(uid).Coordinates;
        var handEntity = Spawn(comp.HandProto, coords);

        comp.HandDuration = TryComp<TimedDespawnComponent>(handEntity, out var timedDespawn)
            ? timedDespawn.Lifetime
            : 1f;

        comp.CurrentPhase = CondemnedPhase.HandActive;
        comp.PhaseTimer = 0f;
    }

    private void UpdateHandPhase(EntityUid uid, float frameTime, CondemnedComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.PhaseTimer += frameTime;

        if (comp.PhaseTimer < comp.HandDuration)
            return;

        DoCondemnedBehavior(uid, comp.ScrambleAfterBanish);

        comp.CurrentPhase = CondemnedPhase.Complete;
    }

    private void DoCondemnedBehavior(EntityUid uid, bool scramble = true, CondemnedComponent? comp = null, bool retry = false)
    {
        TransformComponent? portalXform = null;
        HellPortalExitComponent? targetportal = null;
        if (!Resolve(uid, ref comp))
            return;

        switch (comp)
        {
            case { CondemnedBehavior: CondemnedBehavior.Delete }:
                var query = EntityQueryEnumerator<HellPortalExitComponent, TransformComponent>();
                while (query.MoveNext(out var hellexitportalcomp, out var xform))
                {
                    targetportal = hellexitportalcomp;
                    portalXform = xform;
                    break;
                }

                if (targetportal == null || portalXform == null)
                {
                    if (!_mapLoader.TryLoadMap(comp.HellMapPath,
                        out var map, out var roots,
                        options: new DeserializationOptions { InitializeMaps = true }))
                    {
                        Log.Error($"Failed to load hell map at {comp.HellMapPath}");
                        QueueDel(map);
                        return;
                    }

                    foreach (var root in roots)
                    {
                        if (!HasComp<HellMapComponent>(root))
                            continue;

                        var pos = new EntityCoordinates(root, 0, 0);

                        var exitPortal = Spawn(comp.ExitPortalPrototype, pos);

                        EnsureComp<PortalComponent>(exitPortal, out var hellPortalComp);

                        var newHellMapComp = EnsureComp<HellMapComponent>(root);
                        newHellMapComp.ExitPortal = exitPortal;

                        break;
                    }
                    if (!retry)
                    {
                        DoCondemnedBehavior(uid, scramble, comp, true);
                        return;
                    }
                }
                if (portalXform == null)
                {
                    return;
                }
                // Teleport
                _sharedTransformSystem.SetCoordinates(uid, portalXform.Coordinates);
                break;
            case { CondemnedBehavior: CondemnedBehavior.Banish }:
                if (scramble)
                    _scramble.Scramble(uid);
                _poly.PolymorphEntity(uid, comp.BanishProto);
                break;
        }

        RemCompDeferred(uid, comp);
    }

    private void OnExamined(Entity<CondemnedComponent> condemned, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || condemned.Comp.SoulOwnedNotDevil)
            return;

        var ev = new IsEyesCoveredCheckEvent();
        RaiseLocalEvent(condemned, ev);

        if (ev.IsEyesProtected)
            return;

        args.PushMarkup(Loc.GetString("condemned-component-examined", ("target", Identity.Entity(condemned, EntityManager) )));
    }
}
