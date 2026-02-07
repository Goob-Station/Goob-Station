using Content.Shared._Lavaland.Megafauna.Components.Banana;
using Content.Shared._Lavaland.Megafauna.Events.Banana;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;
using Robust.Server.GameObjects;

namespace Content.Server._Lavaland.Megafauna.Systems.Banana;

public sealed class MegafaunaDirectionalSpawnSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly List<BarrageState> _barrages = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaDirectionalSpawnComponent, SpawnFarSideHandEvent>(OnFarSide);
        SubscribeLocalEvent<MegafaunaDirectionalSpawnComponent, SpawnCloseSideHandEvent>(OnCloseSide);
        SubscribeLocalEvent<MegafaunaDirectionalSpawnComponent, SpawnFarBarrageEvent>(OnFarBarrage);
        SubscribeLocalEvent<MegafaunaDirectionalSpawnComponent, SpawnCloseBarrageEvent>(OnCloseBarrage);
    }

    private void OnFarSide(EntityUid uid, MegafaunaDirectionalSpawnComponent comp, SpawnFarSideHandEvent ev)
    {
        SpawnSideHand(comp, ev.Target, comp.Offset);
        ev.Handled = true;
    }

    private void OnCloseSide(EntityUid uid, MegafaunaDirectionalSpawnComponent comp, SpawnCloseSideHandEvent ev)
    {
        SpawnSideHand(comp, ev.Target, comp.MinOffset);
        ev.Handled = true;
    }

    private void OnFarBarrage(EntityUid uid, MegafaunaDirectionalSpawnComponent comp, SpawnFarBarrageEvent ev)
    {
        StartBarrage(comp, ev.Target, comp.Offset);
        ev.Handled = true;
    }

    private void OnCloseBarrage(EntityUid uid, MegafaunaDirectionalSpawnComponent comp, SpawnCloseBarrageEvent ev)
    {
        StartBarrage(comp, ev.Target, comp.MinOffset);
        ev.Handled = true;
    }


    private void SpawnSideHand(MegafaunaDirectionalSpawnComponent comp, EntityCoordinates targetCoords, float offset)
    {
        var dir = _random.Pick(new[] { -1, 1 }); // left or right
        var direction = new Vector2(dir, 0);

        var proto = dir == 1
            ? comp.GoRightPrototype
            : comp.GoLeftPrototype;

        var spawnPos = targetCoords.Offset(direction * offset);
        Spawn(proto, spawnPos);
    }

    private void StartBarrage(MegafaunaDirectionalSpawnComponent comp, EntityCoordinates target, float offset)
    {
        _barrages.Add(new BarrageState
        {
            Component = comp,
            Target = target,
            Offset = offset,
            Remaining = comp.BarrageCount,
            NextSpawn = _timing.CurTime
        });
    }

    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;

        for (var i = _barrages.Count - 1; i >= 0; i--)
        {
            var barrage = _barrages[i];

            if (time < barrage.NextSpawn)
                continue;

            var dirIndex = _random.Next(0, 4);

            Vector2 direction;
            EntProtoId proto;

            switch (dirIndex)
            {
                case 0:
                    direction = new Vector2(1, 0);
                    proto = barrage.Component.GoRightPrototype;
                    break;
                default:
                    direction = new Vector2(-1, 0);
                    proto = barrage.Component.GoLeftPrototype;
                    break;
            }

            var spawnPos = barrage.Target.Offset(direction * barrage.Offset);
            Spawn(proto, spawnPos);


            barrage.Remaining--;
            barrage.NextSpawn = time + TimeSpan.FromSeconds(barrage.Component.BarrageInterval);

            if (barrage.Remaining <= 0)
                _barrages.RemoveAt(i);
        }
    }

    private sealed class BarrageState
    {
        public MegafaunaDirectionalSpawnComponent Component = default!;
        public EntityCoordinates Target;
        public float Offset;
        public int Remaining;
        public TimeSpan NextSpawn;
    }
}
