// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 unknown <Administrator@DESKTOP-PMRIVVA.kommune.indresogn.no>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Numerics;
using Content.Goobstation.Common.Bingle;
using Content.Goobstation.Shared.Bingle;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Pinpointer;
using Content.Server.Stunnable;
using Content.Shared.Destructible;
using Content.Shared.Destructible;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Destructible;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Humanoid;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bingle;

public sealed class BinglePitSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly BingleSystem _bingle = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly StepTriggerSystem _step = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private EntityQuery<BingleComponent> _query;
    private EntityQuery<BinglePitFallingComponent> _fallingQuery;

    private readonly List<Entity<BinglePitComponent>> _pits = new();
    public static readonly ProtoId<ContentTileDefinition> FloorTile = "FloorBingle";

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<BingleComponent>();
        _fallingQuery = GetEntityQuery<BinglePitFallingComponent>();

        SubscribeLocalEvent<BinglePitComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<BinglePitComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<BinglePitComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<BinglePitComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BinglePitComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<BinglePitComponent, EntRemovedFromContainerMessage>(OnRemovedFromContainer);
        SubscribeLocalEvent<BinglePitFallingComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnRemovedFromContainer(Entity<BinglePitComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        RemCompDeferred<StunnedComponent>(args.Entity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BinglePitFallingComponent>();
        while (query.MoveNext(out var uid, out var falling))
        {
            if (_timing.CurTime < falling.NextDeletionTime)
                continue;

            _containerSystem.Insert(uid, falling.Pit.Comp.Pit);
            EnsureComp<StunnedComponent>(uid); // used stunned to prevent any funny being done inside the pit
            RemCompDeferred(uid, falling);
        }
    }

    private void OnInit(EntityUid uid, BinglePitComponent component, MapInitEvent args)
    {
        if (!Transform(uid).Coordinates.IsValid(EntityManager))
            QueueDel(uid);
        component.Pit = _containerSystem.EnsureContainer<Container>(uid, "pit");

        var coords = Transform(uid).Coordinates;
        for (var i = 0; i < component.StartingBingles; i++)
            Spawn(component.GhostRoleToSpawn, coords);
    }

    private void OnStepTriggered(EntityUid uid, BinglePitComponent component, ref StepTriggeredOffEvent args)
    {
        // dont accept if they are already falling
        if (_fallingQuery.HasComp(args.Tripper))
            return;

        if (_query.HasComp(args.Tripper))
        {
            // only allow dead bingles so you can't suicide
            if (_mob.IsAlive(args.Tripper))
                return;
        }
        else
        {
            // Needs to be at level two or above to allow any non-bingle mobs, dead or alive
            if (component.Level < 2 && HasComp<MobStateComponent>(args.Tripper))
                return;
        }

        StartFalling(uid, component, args.Tripper);

        if (component.BinglePoints >= (component.SpawnNewAt * component.Level))
        {
            SpawnBingle(uid, component);
            component.BinglePoints -= (component.SpawnNewAt * component.Level);
        }
    }

    private void StartFalling(EntityUid uid, BinglePitComponent component, EntityUid tripper, bool playSound = true)
    {
        if (!_mob.IsDead(tripper))
            component.BinglePoints += component.PointsForAlive;
        else
            component.BinglePoints++;
        if (HasComp<HumanoidAppearanceComponent>(tripper))
            component.BinglePoints += component.SpawnNewAt * component.Level; // throwing a humanoid in the pit  will spawn a new bingle

        if (_query.HasComp(tripper))
            component.BinglePoints += (component.SpawnNewAt * component.Level) / 4; //recycling a bingle returns a quarter bingle.

        if (TryComp<PullableComponent>(tripper, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(tripper, pullable, ignoreGrab: true);

        var fall = EnsureComp<BinglePitFallingComponent>(tripper);
        fall.Pit = (uid, component);
        fall.NextDeletionTime = _timing.CurTime + fall.DeletionTime;
        _stun.TryKnockdown(tripper, fall.DeletionTime, false);

        if (playSound)
            _audio.PlayPvs(component.FallingSound, uid);

    }

    private void OnStepTriggerAttempt(EntityUid uid, BinglePitComponent component, ref StepTriggerAttemptEvent args)
        => args.Continue = true;

    public void SpawnBingle(EntityUid uid, BinglePitComponent component)
    {
        Spawn(component.GhostRoleToSpawn, Transform(uid).Coordinates);
        OnSpawnTile(uid, component.Level * 2);

        component.MinionsMade++;
        if (component.MinionsMade < component.UpgradeMinionsAfter)
            return;

        component.MinionsMade = 0;
        component.Level++;
        UpgradeBingles(uid, component);

        // fake ascension at level 5 (insanely hard to get)
        if (component.Level != 5)
            return;

        var ascendSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/ascend_flesh.ogg");
        _chat.DispatchGlobalAnnouncement(Loc.GetString("heretic-ascension-bingle"), Name(uid), true, ascendSound, Color.Pink);
    }

    public void UpgradeBingles(EntityUid uid, BinglePitComponent component)
    {
        var query = EntityQueryEnumerator<BingleComponent>();
        while (query.MoveNext(out var queryUid, out var queryBingleComp))
            if (queryBingleComp.MyPit != null && queryBingleComp.MyPit.Value == uid)
                _bingle.UpgradeBingle(queryUid, queryBingleComp);

        if (component.Level <= component.MaxSize)
            ScaleUpPit(uid, component);

        // make max-size bingle pit ignore gravity
        if (component.Level == component.MaxSize)
            _step.SetIgnoreWeightless(uid, true);

        _popup.PopupEntity(Loc.GetString("bingle-pit-grow"), uid);
    }

    private void OnDestruction(EntityUid uid, BinglePitComponent component, DestructionEventArgs args)
    {
        if (component.Pit != null)
            foreach (var pitUid in _containerSystem.EmptyContainer(component.Pit))
            {
                RemComp<StunnedComponent>(pitUid);
                _stun.TryKnockdown(pitUid, TimeSpan.FromSeconds(2), false);
            }

        RemoveAllBingleGhostRoles(uid, component);//remove all unclaimed ghost roles when pit is destroyed

        //Remove all falling when pit is destroyed, in the small chance someone is in between start and insert
        var query = EntityQueryEnumerator<BinglePitFallingComponent>();
        while (query.MoveNext(out var fallingUid, out var fallingComp))
        {
            if (fallingComp.Pit.Owner == uid)
                RemCompDeferred(fallingUid, fallingComp);
        }
    }

    public void RemoveAllBingleGhostRoles(EntityUid uid, BinglePitComponent component)
    {
        var query = EntityQueryEnumerator<GhostRoleMobSpawnerComponent>();
        while (query.MoveNext(out var queryGRMSUid, out var queryGRMScomp))
            if (queryGRMScomp.Prototype == "MobBingle")
                if (Transform(uid).Coordinates == Transform(queryGRMSUid).Coordinates)
                    QueueDel(queryGRMSUid); // remove any unspanned bingle when pit is destroyed
    }
    private void OnAttacked(EntityUid uid, BinglePitComponent component, AttackedEvent args)
    {
        if (_containerSystem.ContainsEntity(uid, args.User))
            EnsureComp<StunnedComponent>(args.User);
    }

    private void OnUpdateCanMove(EntityUid uid, BinglePitFallingComponent component, UpdateCanMoveEvent args)
        => args.Cancel();

    private void ScaleUpPit(EntityUid uid, BinglePitComponent component)
    {
        EnsureComp<ScaleVisualsComponent>(uid);

        _appearance.SetData(uid, ScaleVisuals.Scale, Vector2.One * component.Level);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        var query = AllEntityQuery<BinglePitComponent>();

        _pits.Clear();
        while (query.MoveNext(out var uid, out var comp))
            _pits.Add((uid, comp));

        if (_pits.Count == 0)
            return;

        ev.AddLine("");

        foreach (var ent in _pits)
        {
            var (uid, comp) = ent;

            var location = "Unknown";
            var mapCoords = _transform.ToMapCoordinates(Transform(uid).Coordinates);
            if (_navMap.TryGetNearestBeacon(mapCoords, out var beacon, out _))
                location = beacon?.Comp?.Text!;

            var points = comp.BinglePoints + (comp.MinionsMade * comp.SpawnNewAt) * comp.Level;

            ev.AddLine(Loc.GetString("bingle-pit-end-of-round",
                ("location", location),
                ("level", comp.Level),
                ("points", points)));
        }

        ev.AddLine("");
    }

    private void OnSpawnTile(EntityUid uid, float radius)
    {
        var tgtPos = Transform(uid);
        if (tgtPos.GridUid is not { } gridUid || !TryComp(gridUid, out MapGridComponent? mapGrid))
            return;

        var tileEnumerator = _map.GetLocalTilesEnumerator(gridUid, mapGrid, new Box2(tgtPos.Coordinates.Position + new Vector2(-radius, -radius), tgtPos.Coordinates.Position + new Vector2(radius, radius)));
        var convertTile = (ContentTileDefinition) _tiledef[FloorTile];

        while (tileEnumerator.MoveNext(out var tile))
        {
            if (tile.Tile.TypeId == convertTile.TileId)
                continue;
            if (_turf.GetContentTileDefinition(tile).Name != convertTile.Name &&
                _random.Prob(0.1f)) // 10% probability to transform tile
            {
                _tile.ReplaceTile(tile, convertTile);
                _tile.PickVariant(convertTile);
            }
        }

    }

}
