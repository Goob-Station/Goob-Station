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
using Content.Shared._Lavaland.Hierophant.Components;
using Content.Shared._Lavaland.Megafauna;
using Content.Shared._Lavaland.Tile;
using Content.Shared._Lavaland.Tile.Shapes;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

// ReSharper disable EnforceForStatementBraces
namespace Content.Shared._Lavaland.Hierophant.Systems;

public sealed class HierophantSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TileShapeSystem _tile = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantBossComponent, MegafaunaShutdownEvent>(OnHierophantDeinit);
    }

    private readonly SoundPathSpecifier _defaultBlinkSound = new("/Audio/Magic/blink.ogg");

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<HierophantBossComponent>();
        while (query.MoveNext(out var uid, out var hiero))
        {
            if (hiero.ArenaReturnTime == null
                || !(hiero.ArenaReturnTime > curTime)
                || hiero.ConnectedFieldGenerator == null
                || TerminatingOrDeleted(hiero.ConnectedFieldGenerator))
                continue;

            var field = hiero.ConnectedFieldGenerator.Value;
            var position = _xform.GetMapCoordinates(field);
            _xform.SetMapCoordinates(uid, position);
            hiero.ArenaReturnTime = null;
        }

        var blinkQuery = EntityQueryEnumerator<HierophantActiveBlinkComponent>();
        while (blinkQuery.MoveNext(out var uid, out var blink))
        {
            if (blink.DefaultBlinkTime == null
                || !(blink.DefaultBlinkTime > curTime)
                || blink.BlinkDummy == null
                || TerminatingOrDeleted(blink.BlinkDummy))
                continue;

            _xform.SetCoordinates(uid, Transform(blink.BlinkDummy.Value).Coordinates);
            PredictedQueueDel(blink.BlinkDummy.Value);
            RemComp(uid, blink);
        }
    }

    private void OnHierophantDeinit(Entity<HierophantBossComponent> ent, ref MegafaunaShutdownEvent args)
        => ent.Comp.ArenaReturnTime = _timing.CurTime + ent.Comp.ArenaReturnDelay;

    public void SpawnChasers(EntityUid uid, EntProtoId tileId, float speed = 3f, int maxSteps = 20, EntityUid? target = null, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            var chaser = PredictedSpawnAtPosition(tileId, Transform(uid).Coordinates);
            if (!TryComp<HierophantChaserComponent>(chaser, out var chasercomp))
                continue;

            chasercomp.Target = target;
            chasercomp.MaxSteps *= maxSteps;
            chasercomp.Speed += speed;
        }
    }

    public void Blink(
        EntityUid ent,
        EntProtoId damageBoxId,
        TileShape shape,
        EntityCoordinates coords,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
    {
        var dummy = PredictedSpawnAtPosition(null, coords);
        _tile.SpawnTileShape(shape, dummy, damageBoxId, out _);
        _tile.SpawnTileShape(shape, dummy, damageBoxId, out _);

        // TODO add cool shaders on clientside
        _audio.PlayPvs(sound ?? _defaultBlinkSound,
            Transform(ent).Coordinates,
            AudioParams.Default.WithMaxDistance(12f));

        var blinkComp = EnsureComp<HierophantActiveBlinkComponent>(ent);
        blinkComp.DefaultBlinkTime = _timing.CurTime + duration ?? blinkComp.DefaultBlinkTime;
    }

    public void BlinkToTarget(
        EntityUid ent,
        EntProtoId damageBoxId,
        TileShape shape,
        EntityUid target,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
        => Blink(ent, damageBoxId, shape, Transform(ent).Coordinates, duration);

    public void BlinkRandom(
        EntityUid uid,
        EntProtoId damageBoxId,
        TileShape shape,
        SoundSpecifier? sound = null)
    {
        var xform = Transform(uid);
        if (xform.GridUid == null)
            return;

        var newPos = new EntityCoordinates();
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

        Blink(uid, damageBoxId, shape, newPos);
    }
}
