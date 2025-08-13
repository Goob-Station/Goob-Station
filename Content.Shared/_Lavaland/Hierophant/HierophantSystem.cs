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
using Robust.Shared.Random;
using Content.Shared._Lavaland.Hierophant.Components;
using Content.Shared._Lavaland.HierophantClub;
using Content.Shared._Lavaland.Megafauna;
using Content.Shared._Lavaland.Movement;
using Content.Shared._Lavaland.TileChaser;
using Content.Shared.Coordinates.Helpers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Hierophant;

public sealed class HierophantSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantBossComponent, MapInitEvent>(OnBossMapInit);
        SubscribeLocalEvent<HierophantBossComponent, MegafaunaShutdownEvent>(OnBossMegafaunaShutdown);

        SubscribeLocalEvent<HierophantMagicComponent, HierophantBeatActionEvent>(OnHierophantBeat);
        SubscribeLocalEvent<HierophantMagicComponent, HierophantChasersActionEvent>(OnChasersAction);
        SubscribeLocalEvent<HierophantMagicComponent, HierophantBlinkActionEvent>(OnBlinkAction);
        SubscribeLocalEvent<HierophantMagicComponent, HierophantAttackActionEvent>(OnAttackAction);
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

        var immuneQuery = EntityQueryEnumerator<HierophantImmuneComponent>();
        while (immuneQuery.MoveNext(out var uid, out var immunity))
        {
            if (immunity.EndTime == null
                || _timing.CurTime < immunity.EndTime)
                continue;

            RemComp(uid, immunity);
        }
    }

    private void OnBossMapInit(Entity<HierophantBossComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsClient)
            return;

        ent.Comp.ConnectedMarker = Spawn(ent.Comp.MarkerId);
        Dirty(ent);
    }

    private void OnBossMegafaunaShutdown(Entity<HierophantBossComponent> ent, ref MegafaunaShutdownEvent args)
    {
        if (ent.Comp.ConnectedMarker == null)
            return;

        var coords = Transform(ent.Comp.ConnectedMarker.Value).Coordinates;
        Blink(ent.Owner, ent.Comp.BlinkShapeSpawner, coords, ent.Comp.ArenaReturnDelay);
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
        if (args.Handled
            || _net.IsClient) // No spawn/random prediction
            return;

        var calculator = new MegafaunaCalculationBaseArgs(ent.Owner, EntityManager, _protoMan, _random.GetRandom());

        var speed = args.SpeedSelector.Get(calculator);
        var steps = args.StepsSelector.GetRounded(calculator);
        var amount = args.AmountSelector.GetRounded(calculator);

        SpawnChasers(ent.Owner, args.DamageTile, speed, steps, amount, args.Target);
        args.Handled = true;
    }

    private void OnBlinkAction(Entity<HierophantMagicComponent> ent, ref HierophantBlinkActionEvent args)
    {
        if (args.Handled)
            return;

        Blink(ent, args.Spawn, args.Target, args.Duration, args.Sound);
        args.Handled = true;
    }

    private void OnAttackAction(Entity<HierophantMagicComponent> ent, ref HierophantAttackActionEvent args)
    {
        if (args.Handled
            || _xform.GetGrid(args.Target) == null)
            return;

        PredictedSpawnAtPosition(args.Spawn, args.Target);
        args.Handled = true;
    }

    public void SpawnChasers(EntityUid uid, EntProtoId tileId, float speed, int maxSteps, int amount, EntityUid? target = null)
    {
        if (_net.IsClient)
            return; // Spawn prediction for now doesn't work properly

        // Always should be snapped to the center of the tile
        var coords = Transform(uid).Coordinates.SnapToGrid(EntityManager, _mapMan);

        for (int i = 0; i < amount; i++)
        {
            var chaser = SpawnAtPosition(tileId, coords);
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
        EntProtoId spawnId,
        EntityCoordinates coords,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
    {
        PredictedSpawnAtPosition(spawnId, coords);
        PredictedSpawnAtPosition(spawnId, Transform(ent).Coordinates);

        var blinkComp = EnsureComp<HierophantActiveBlinkComponent>(ent);
        blinkComp.BlinkTime = _timing.CurTime + duration ?? blinkComp.DefaultDelay;
        blinkComp.Coordinates = coords;
        blinkComp.Sound = sound;
        Dirty(ent, blinkComp);
    }

    public void Blink(
        EntityUid ent,
        EntProtoId damageBoxId,
        EntityUid target,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
        => Blink(ent, damageBoxId, Transform(target).Coordinates, duration, sound);
}
