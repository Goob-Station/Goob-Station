using Content.Shared._Lavaland.Megafauna.Banana.Components;
using Content.Shared._Lavaland.Megafauna.Banana.Events;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server._Lavaland.Megafauna.Banana.Systems;

public sealed class MegafaunaDirectionalSpawnSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly List<BarrageState> _barrages = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaDirectionalSpawnComponent, SpawnFarSideEvent>(OnFarSide);
        SubscribeLocalEvent<MegafaunaDirectionalSpawnComponent, SpawnCloseSideEvent>(OnCloseSide);
        SubscribeLocalEvent<MegafaunaDirectionalSpawnComponent, SpawnFarBarrageEvent>(OnFarBarrage);
        SubscribeLocalEvent<MegafaunaDirectionalSpawnComponent, SpawnCloseBarrageEvent>(OnCloseBarrage);
    }

    private void OnFarSide(Entity<MegafaunaDirectionalSpawnComponent> ent, ref SpawnFarSideEvent args)
    {
        SpawnSide(ent.Comp, args.Target, ent.Comp.Offset);
        args.Handled = true;
    }

    private void OnCloseSide(Entity<MegafaunaDirectionalSpawnComponent> ent, ref SpawnCloseSideEvent args)
    {
        SpawnSide(ent.Comp, args.Target, ent.Comp.MinOffset);
        args.Handled = true;
    }

    private void OnFarBarrage(Entity<MegafaunaDirectionalSpawnComponent> ent, ref SpawnFarBarrageEvent args)
    {
        StartBarrage(ent.Comp, args.Target, ent.Comp.Offset);
        args.Handled = true;
    }

    private void OnCloseBarrage(Entity<MegafaunaDirectionalSpawnComponent> ent, ref SpawnCloseBarrageEvent args)
    {
        StartBarrage(ent.Comp, args.Target, ent.Comp.MinOffset);
        args.Handled = true;
    }

    /// <summary>
    /// Rolls a side (50/50 left or right) and returns the direction vector and matching prototype.
    /// </summary>
    private (Vector2 Direction, EntProtoId Prototype) PickSide(MegafaunaDirectionalSpawnComponent comp)
    {
        if (_random.Prob(0.5f))
        {
            return (new Vector2(1, 0), comp.RightPrototype);
        }

        return (new Vector2(-1, 0), comp.LeftPrototype);
    }

    private void SpawnSide(MegafaunaDirectionalSpawnComponent comp, EntityCoordinates targetCoords, float offset)
    {
        var (direction, proto) = PickSide(comp);
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
            NextSpawn = _timing.CurTime,
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        for (var i = _barrages.Count - 1; i >= 0; i--)
        {
            var barrage = _barrages[i];

            if (time < barrage.NextSpawn)
            {
                continue;
            }

            var (direction, proto) = PickSide(barrage.Component);
            var spawnPos = barrage.Target.Offset(direction * barrage.Offset);
            Spawn(proto, spawnPos);

            barrage.Remaining--;
            barrage.NextSpawn = time + TimeSpan.FromSeconds(barrage.Component.BarrageInterval);

            if (barrage.Remaining <= 0)
            {
                _barrages.RemoveAt(i);
            }
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
