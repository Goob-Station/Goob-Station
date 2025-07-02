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
using System.Numerics;
using System.Threading.Tasks;
using Content.Server._Lavaland.Hierophant.Components;
using Content.Server._Lavaland.Megafauna;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

// ReSharper disable EnforceForStatementBraces
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Content.Server._Lavaland.Hierophant;

public sealed class HierophantSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HierophantBossComponent, MegafaunaShutdownEvent>(OnHierophantDeinit);
    }

    public override void Update(float frameTime)
    {
        // ALL I WANTED IS TO GET RID OF TIMER.SPAWN.
        // AND WHAT I GOT? 2 USELESS COMPONENTS AND 3 FUCKING UPDATE LOOPS!!!
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<HierophantBossComponent>();
        while (query.MoveNext(out var uid, out var hiero))
        {
            if (hiero.ArenaReturnTime != null
                && hiero.ArenaReturnTime > curTime
                && hiero.ConnectedFieldGenerator != null)
            {
                var field = hiero.ConnectedFieldGenerator.Value;
                var position = _xform.GetMapCoordinates(field);
                _xform.SetMapCoordinates(uid, position);
                hiero.ArenaReturnTime = null;
            }
        }

        var blinkQuery = EntityQueryEnumerator<HierophantActiveBlinkComponent>();
        while (blinkQuery.MoveNext(out var uid, out var blink))
        {
            if (blink.DefaultBlinkTime != null
                && blink.DefaultBlinkTime > curTime
                && blink.BlinkDummy != null)
            {
                // TODO add cool shaders on clientside
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), Transform(uid).Coordinates, AudioParams.Default.WithMaxDistance(12f));
                _xform.SetCoordinates(uid, Transform(blink.BlinkDummy.Value).Coordinates); // CROSS MAP TP!!!
                QueueDel(blink.BlinkDummy.Value);
                RemComp(uid, blink);
            }
        }

        var attackQuery = EntityQueryEnumerator<HierophantAttackSpawnerComponent>();
        while (attackQuery.MoveNext(out var uid, out var attack))
        {
            attack.Accumulator -= frameTime;
            if (attack.Accumulator > 0f)
                continue;
            attack.Accumulator = attack.AttackDelay;

            if (attack.Counter > attack.FinalCounter)
            {
                QueueDel(uid);
                continue;
            }

            HierophantRepeatAttackType attackType;
            // Basically this thing makes this array looped
            if (attack.AttacksOrder.Length <= attack.Counter)
                attackType = attack.AttacksOrder[attack.Counter - attack.AttacksOrder.Length];
            else
                attackType = attack.AttacksOrder[attack.Counter];

            switch (attackType)
            {
                case HierophantRepeatAttackType.CrossRook:
                    SpawnCrossRook(uid, attack.TileId);
                    break;
                case HierophantRepeatAttackType.CrossBishop:
                    SpawnCrossBishop(uid, attack.TileId);
                    break;
                case HierophantRepeatAttackType.CrossQueen:
                    SpawnCrossQueen(uid, attack.TileId);
                    break;
                case HierophantRepeatAttackType.BoxHollow:
                    SpawnDamageBox(uid, attack.TileId, attack.Counter);
                    break;
                case HierophantRepeatAttackType.BoxFilled:
                    SpawnDamageBox(uid, attack.TileId, attack.Counter, false);
                    break;
            }

            attack.Counter++;
        }
    }

    private void OnHierophantDeinit(Entity<HierophantBossComponent> ent, ref MegafaunaShutdownEvent args)
    {
        ent.Comp.ArenaReturnTime = _timing.CurTime + ent.Comp.ArenaReturnDelay;
    }

    /// <summary>
    /// Sets up a dummy entity that creates an expanding square pattern.
    /// </summary>
    public void SetupSquarePattern(EntityUid target, EntProtoId tileId, float speed = 0.7f, int radius = 5)
    {
        var dummyEnt = Spawn(null, Transform(target).Coordinates);
        var originComp = EnsureComp<HierophantAttackSpawnerComponent>(dummyEnt);

        originComp.FinalCounter = radius;
        originComp.TileId = tileId;
        originComp.AttackDelay = speed;
        originComp.AttacksOrder = new HierophantRepeatAttackType[1];
        originComp.AttacksOrder[0] = HierophantRepeatAttackType.BoxFilled;
    }

    private readonly List<HierophantRepeatAttackType> _crossAttacks = new()
    {
        HierophantRepeatAttackType.CrossRook,
        HierophantRepeatAttackType.CrossBishop,
    };

    private readonly List<HierophantRepeatAttackType> _crossAttacksQueen = new()
    {
        HierophantRepeatAttackType.CrossRook,
        HierophantRepeatAttackType.CrossBishop,
        HierophantRepeatAttackType.CrossQueen,
    };

    public void SetupCrossPattern(EntityUid target, EntProtoId tileId, float speed = 1.4f, int amount = 1, bool allowBoth = false)
    {
        var dummyEnt = Spawn(null, Transform(target).Coordinates);
        var originComp = EnsureComp<HierophantAttackSpawnerComponent>(dummyEnt);

        originComp.FinalCounter = amount;
        originComp.TileId = tileId;
        originComp.AttackDelay = speed;
        originComp.AttacksOrder = new HierophantRepeatAttackType[originComp.FinalCounter];

        for (int i = 0; i < originComp.FinalCounter; i++)
            originComp.AttacksOrder[i] = _random.Pick(allowBoth ? _crossAttacksQueen : _crossAttacks);
    }

    public void SpawnChasers(EntityUid uid, EntProtoId tileId, float speed = 3f, int maxSteps = 20, EntityUid? target = null, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            var chaser = Spawn(tileId, Transform(uid).Coordinates);
            if (!TryComp<HierophantChaserComponent>(chaser, out var chasercomp))
                continue;

            chasercomp.Target = target;
            chasercomp.MaxSteps *= maxSteps;
            chasercomp.Speed += speed;
        }
    }

    public void SpawnDamageBox(EntityUid target, EntProtoId damageBoxId, int range = 0, bool hollow = true, int borderRange = 1)
    {
        var xform = Transform(target);
        if (range == 0)
        {
            Spawn(damageBoxId, xform.Coordinates);
            return;
        }

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid)
            || !_xform.TryGetGridTilePosition(target, out var tilePos))
            return;

        var box = hollow ? TileHelperMethods.MakeBoxHollow(tilePos, range) : TileHelperMethods.MakeBox(tilePos, range);

        SpawnPatternAsync(damageBoxId, (xform.GridUid.Value, grid), box);
    }

    public void SpawnBoxHell(EntityUid target, EntProtoId damageBoxId, float filledTileProb, int range = 11)
    {
        var xform = Transform(target);
        if (range == 0)
            return;

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid)
            || !_xform.TryGetGridTilePosition(target, out var tilePos))
            return;

        var box = TileHelperMethods.MakeBoxRandom(tilePos, range, _random, filledTileProb);

        SpawnPatternAsync(damageBoxId, (xform.GridUid.Value, grid), box);
    }

    public void SpawnCrossRook(EntityUid target, EntProtoId damageBoxId, int range = 10)
    {
        var xform = Transform(target);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid) ||
            !_xform.TryGetGridTilePosition(target, out var tilePos))
            return;

        var cross = TileHelperMethods.MakeCross(tilePos, range);

        SpawnPatternAsync(damageBoxId, (xform.GridUid.Value, grid), cross);
    }

    public void SpawnCrossBishop(EntityUid target, EntProtoId damageBoxId, int range = 10)
    {
        var xform = Transform(target);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid) ||
            !_xform.TryGetGridTilePosition(target, out var tilePos))
            return;

        var diagcross = TileHelperMethods.MakeCrossDiagonal(tilePos, range);

        SpawnPatternAsync(damageBoxId, (xform.GridUid.Value, grid), diagcross);
    }

    public void SpawnCrossQueen(EntityUid target, EntProtoId damageBoxId, int range = 10)
    {
        var xform = Transform(target);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid) ||
            !_xform.TryGetGridTilePosition(target, out var tilePos))
            return;

        var cross = TileHelperMethods.MakeCross(tilePos, range).ToHashSet();
        var diagcross = TileHelperMethods.MakeCrossDiagonal(tilePos, range).ToHashSet();
        var both = cross.Concat(diagcross).ToHashSet();

        SpawnPatternAsync(damageBoxId, (xform.GridUid.Value, grid), both.ToList());
    }

    public void Blink(EntityUid ent, EntProtoId damageBoxId, MapCoordinates worldPos, TimeSpan? duration = null)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var dummy = Spawn(null, worldPos);
        SpawnDamageBox(ent, damageBoxId, 1, false);
        SpawnDamageBox(dummy, damageBoxId, 1, false);

        var blinkComp = EnsureComp<HierophantActiveBlinkComponent>(ent);
        blinkComp.DefaultBlinkTime = _timing.CurTime + duration ?? blinkComp.DefaultBlinkTime;
    }

    public void BlinkToTarget(EntityUid ent, EntProtoId damageBoxId, EntityUid target, TimeSpan? duration = null)
        => Blink(ent, damageBoxId, _xform.GetMapCoordinates(target), duration);

    public void BlinkRandom(EntityUid uid, EntProtoId damageBoxId)
    {
        var vector = new Vector2();

        var xform = Transform(uid);
        if (xform.GridUid == null)
            return;

        for (var i = 0; i < 20; i++)
        {
            var randomVector = _random.NextVector2(4f, 4f);
            var position = _xform.GetWorldPosition(uid) + randomVector;
            var checkBox = Box2.CenteredAround(position, new Vector2i(2, 2));

            var ents = _map.GetAnchoredEntities(xform.GridUid.Value, Comp<MapGridComponent>(xform.GridUid.Value), checkBox);
            if (!ents.Any())
                vector = position;
        }

        var mapPos = new MapCoordinates(vector + new Vector2(0.5f, 0.5f), xform.MapID);
        Blink(uid, damageBoxId, mapPos);
    }

    // This thing is async because that way all entities spawn all at once and (probably?) lag less.
    private async Task SpawnPatternAsync(EntProtoId entityId, Entity<MapGridComponent> grid, List<Vector2i> gridTiles)
    {
        // ReSharper disable once EnforceForeachStatementBraces
        foreach (var tile in gridTiles)
            Spawn(entityId, _map.GridTileToWorld(grid.Owner, grid.Comp, tile));
    }
}
