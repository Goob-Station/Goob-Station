using System.Numerics;
using Content.Shared._Lavaland.EntityShapes;
using Content.Shared._Lavaland.EntityShapes.Shapes;
using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Spawn safe zone, then spawn ring of beams that slowly tightens up towards the safe zone.
/// You've seen this trick a million times in videogames, should be pretty obvious how it works.
/// </summary>
public sealed class ORTConvergenceSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityShapeSystem _shapes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ORTConvergenceComponent, ORTConvergenceActionEvent>(OnAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<ORTConvergenceComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Active || _timing.CurTime < comp.NextWaveTime)
                continue;

            // 1 = waves done
            var progress = (float) comp.CurrentWave / comp.WaveCount;

            // larp
            var waveRadius = MathHelper.Lerp(comp.StartRadius, comp.SafeZoneRadius, progress);

            var waveCount = Math.Max(comp.MinCount, (int) Math.Round(comp.Count * (waveRadius / comp.StartRadius)));

            if (comp.SafeZoneEntity is null || !Exists(comp.SafeZoneEntity.Value))
            {
                Finish(uid, comp);
                continue;
            }

            var safeZoneCoords = Transform(comp.SafeZoneEntity.Value).Coordinates;
            var shape = new RingEntityShape { Radius = waveRadius, Size = waveCount };
            _shapes.SpawnEntityShape(shape, safeZoneCoords, comp.WarningPrototype, out _);

            comp.CurrentWave++;

            if (comp.CurrentWave > comp.WaveCount)
            {
                Finish(uid, comp);
            }
            else
            {
                comp.NextWaveTime = _timing.CurTime + comp.WaveDelay;
            }
        }
    }

    private void OnAction(EntityUid uid, ORTConvergenceComponent comp, ORTConvergenceActionEvent args)
    {
        if (args.Handled || !_net.IsServer)
            return;

        var bossPos = _transform.GetWorldPosition(uid);
        var safeZonePos = GetSafeZonePosition(bossPos, comp.MinDistance, comp.MaxDistance);

        // Spawn the safe zone
        var mapId = Transform(uid).MapID;
        var spawnCoords = new MapCoordinates(safeZonePos, mapId);
        comp.SafeZoneEntity = Spawn(comp.SafeZonePrototype, spawnCoords);

        comp.Active = true;
        comp.CurrentWave = 0;
        comp.NextWaveTime = _timing.CurTime + comp.InitialDelay;

        args.Handled = true;
    }

    // Random distance between max and min distance
    private Vector2 GetSafeZonePosition(Vector2 bossPos, float minDist, float maxDist)
    {
        var angle = _random.NextFloat() * MathF.Tau;
        var distance = _random.NextFloat() * (maxDist - minDist) + minDist;
        return bossPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
    }

    private void Finish(EntityUid uid, ORTConvergenceComponent comp)
    {
        comp.Active = false;
        comp.CurrentWave = 0;

        // kill it with fire
        if (comp.SafeZoneEntity.HasValue && Exists(comp.SafeZoneEntity.Value))
            QueueDel(comp.SafeZoneEntity.Value);

        comp.SafeZoneEntity = null;
    }
}
