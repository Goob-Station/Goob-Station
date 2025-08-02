// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using System.Linq;
using Content.Shared._Lavaland.EntityShapes;
using Content.Shared._Lavaland.EntityShapes.Shapes;
using Content.Shared._Lavaland.Hierophant.Components;
using Content.Shared._Lavaland.HierophantClub;
using Content.Shared._Lavaland.Megafauna;
using Content.Shared._Lavaland.Movement;
using Content.Shared._Lavaland.TileChaser;
using Content.Shared.Chat;
using Content.Shared.Coordinates.Helpers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

// ReSharper disable EnforceForStatementBraces
namespace Content.Shared._Lavaland.Hierophant.Systems;

public sealed class HierophantSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityShapeSystem _shape = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantBossComponent, MegafaunaShutdownEvent>(OnHierophantDeinit);

        SubscribeLocalEvent<HierophantMagicComponent, HierophantBeatActionEvent>(OnHierophantBeat);
        SubscribeLocalEvent<HierophantMagicComponent, HierophantChasersActionEvent>(OnChasersAction);
        SubscribeLocalEvent<HierophantMagicComponent, HierophantBlinkActionEvent>(OnBlinkAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var blinkQuery = EntityQueryEnumerator<HierophantActiveBlinkComponent>();
        while (blinkQuery.MoveNext(out var uid, out var blink))
        {
            if (blink.BlinkTime == null
                || _timing.CurTime < blink.BlinkTime
                || !blink.Coordinates.IsValid(EntityManager))
                continue;

            _xform.SetCoordinates(uid, blink.Coordinates.SnapToGrid(EntityManager, _mapMan));
            _audio.PlayPredicted(blink.Sound, blink.Coordinates, uid);
            RemComp(uid, blink);
        }
    }

    private void OnHierophantDeinit(Entity<HierophantBossComponent> ent, ref MegafaunaShutdownEvent args)
    {
        if (ent.Comp.ConnectedFieldGenerator == null)
            return;

        var coords = Transform(ent.Comp.ConnectedFieldGenerator.Value).Coordinates;
        Blink(ent.Owner, ent.Comp.DamageTile, ent.Comp.TeleportShape, coords, ent.Comp.ArenaReturnDelay);
    }

    private void OnHierophantBeat(Entity<HierophantMagicComponent> ent, ref HierophantBeatActionEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<HierophantBeatComponent>(args.Target))
            RemComp<HierophantBeatComponent>(args.Target);
        else
            EnsureComp<HierophantBeatComponent>(args.Target);

        args.Handled = true;
    }

    private void OnChasersAction(Entity<HierophantMagicComponent> ent, ref HierophantChasersActionEvent args)
    {
        SpawnChasers();
    }

    public void SpawnChasers(EntityUid uid, EntProtoId tileId, float speed, int maxSteps, int amount, EntityUid? target = null)
    {
        //if (_net.IsClient)
            //return; // Spawn prediction for now doesn't work properly

        // Always should be snapped to the center of the tile
        var coords = Transform(uid).Coordinates.SnapToGrid(EntityManager, _mapMan);

        for (int i = 0; i < amount; i++)
        {
            var chaser = PredictedSpawnAtPosition(tileId, coords);
            if (!TryComp<TileChaserComponent>(chaser, out var chaserComp))
                continue;

            chaserComp.Target = target;
            chaserComp.MaxSteps = maxSteps;
            chaserComp.Speed = speed;
            Dirty(chaser, chaserComp);
        }
    }

    public void Blink(
        EntityUid ent,
        EntProtoId damageBoxId,
        EntityShape shape,
        EntityCoordinates coords,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
    {
        _shape.SpawnTileShape(shape, Transform(ent).Coordinates, damageBoxId, out _);
        _shape.SpawnTileShape(shape, coords, damageBoxId, out _);

        var blinkComp = EnsureComp<HierophantActiveBlinkComponent>(ent);
        blinkComp.BlinkTime = _timing.CurTime + duration ?? blinkComp.BlinkDelay;
        blinkComp.Coordinates = coords;
        blinkComp.Sound = sound;
        Dirty(ent, blinkComp);
    }

    public void BlinkToTarget(
        EntityUid ent,
        EntProtoId damageBoxId,
        EntityShape shape,
        EntityUid target,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
        => Blink(ent, damageBoxId, shape, Transform(target).Coordinates, duration, sound);

    public void BlinkRandom(
        EntityUid uid,
        EntProtoId damageBoxId,
        EntityShape shape,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
    {
        var xform = Transform(uid);
        if (xform.GridUid == null)
            return;

        EntityCoordinates? newPos = null;
        for (var i = 0; i < 20; i++)
        {
            var randomVector = _random.NextVector2(4f, 4f);
            var position = xform.Coordinates.Offset(randomVector)
                .AlignWithClosestGridTile(entityManager: EntityManager, mapManager: _mapMan);
            var checkBox = Box2.CenteredAround(position.Position, new Vector2i(2, 2));

            var ents = _map.GetAnchoredEntities(xform.GridUid.Value, Comp<MapGridComponent>(xform.GridUid.Value), checkBox);
            if (!ents.Any())
                newPos = position;
        }

        newPos ??= xform.Coordinates;
        Blink(uid, damageBoxId, shape, newPos.Value, duration, sound);
    }

    /// <summary>
    /// Tries to Blink to some target, and fallbacks to blinking
    /// in a random direction if target is null.
    /// </summary>
    public void TryBlink(
        EntityUid ent,
        EntProtoId damageBoxId,
        EntityShape shape,
        EntityUid? target,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
    {
        if (target != null)
            BlinkToTarget(ent, damageBoxId, shape, target.Value, duration, sound);
        else
            BlinkRandom(ent, damageBoxId, shape);
    }

    /// <summary>
    /// Tries to Blink to some target, and fallbacks to blinking
    /// in a random direction if target is null.
    /// </summary>
    public void TryBlink(
        EntityUid ent,
        EntProtoId damageBoxId,
        EntityShape shape,
        EntityCoordinates? target,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
    {
        if (target != null)
            Blink(ent, damageBoxId, shape, target.Value, duration, sound);
        else
            BlinkRandom(ent, damageBoxId, shape);
    }
}
