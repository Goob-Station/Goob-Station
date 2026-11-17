using System.Numerics;
using Content.Goobstation.Shared.Nullspace;

namespace Content.Goobstation.Client.Nullspace;

public sealed partial class NullspaceSystem
{
    private const int LightRadius = 13;
    private const float LampRange = 20f;
    private const float LampHeight = NullspaceParams.WallHeight - 0.3f;

    private Dictionary<Vector2i, float> _levels = new();
    private Dictionary<Vector2i, float> _nextLevels = new();
    private readonly List<(float Distance, Vector2i Cell, float Level)> _lit = new();

    private Vector3? _nearestLit;
    private float _nearestLevel;

    private (Vector3? Nearest, float Level) CollectLights(
        NullspaceFrame frame,
        NullspaceMapComponent map,
        NullspaceMaze maze,
        Vector2 viewer,
        float time,
        Vector2? clownPos,
        bool events)
    {
        var viewerCell = new Vector2i((int) MathF.Floor(viewer.X / Cell), (int) MathF.Floor(viewer.Y / Cell));
        var darkness = NullspaceParams.Darkness(GameTime(map, _timing.CurTime));

        frame.Fixtures.Clear();
        _lit.Clear();
        if (events)
            _nextLevels.Clear();

        for (var dy = -LightRadius; dy <= LightRadius; dy++)
        {
            for (var dx = -LightRadius; dx <= LightRadius; dx++)
            {
                var cell = viewerCell + new Vector2i(dx, dy);
                if (maze.Fixture(cell.X, cell.Y, darkness) is not { } kind)
                    continue;

                var center = CellCenter(cell);
                var distance = (center - viewer).Length();
                if (distance > NullspaceParams.LightActiveRange)
                    continue;

                var level = Brightness(map, maze, cell, kind, time, clownPos);
                if (events)
                {
                    var previous = _levels.GetValueOrDefault(cell, level);
                    if (kind == NullspaceFixtureKind.Flicker && previous > 0.5f && level < 0.1f && distance < 10f)
                        OnFlickerOff(center);

                    _nextLevels[cell] = level;
                }

                frame.Fixtures.Add(new NullspaceFixtureVisual(cell, level, maze.FixtureAlongX(cell.X, cell.Y)));

                if (level > 0.02f)
                    _lit.Add((distance, cell, level));
            }
        }

        if (events)
            (_levels, _nextLevels) = (_nextLevels, _levels);

        _lit.Sort(static (a, b) => a.Distance.CompareTo(b.Distance));
        for (var i = 0; i < frame.Lamps.Length; i++)
        {
            if (i >= _lit.Count || _lit[i].Distance > LampRange)
            {
                frame.Lamps[i] = Vector4.Zero;
                continue;
            }

            var center = CellCenter(_lit[i].Cell);
            frame.Lamps[i] = new Vector4(center.X, center.Y, LampHeight, 30f * _lit[i].Level);
        }

        if (_lit.Count == 0)
            return (null, 0f);

        var nearest = CellCenter(_lit[0].Cell);
        return (new Vector3(nearest.X, nearest.Y, NullspaceParams.WallHeight - 0.1f), _lit[0].Level);
    }

    private static float Brightness(NullspaceMapComponent map, NullspaceMaze maze, Vector2i cell, NullspaceFixtureKind kind, float time, Vector2? clownPos)
    {
        if (kind == NullspaceFixtureKind.Dead || map.DeadLights.Contains(cell))
            return 0f;

        var phase = maze.FixturePhase(cell.X, cell.Y);
        if (clownPos is { } clown && (clown - CellCenter(cell)).Length() < NullspaceParams.LightDisturbRange)
        {
            var step = (int) MathF.Floor(time * 22f + phase * 50f);
            var roll = NullspaceMaze.Hash(0x51, step, cell.X, cell.Y) % 100 / 100f;
            return roll < 0.6f ? 0f : roll < 0.8f ? 0.25f : 0.8f;
        }

        if (kind == NullspaceFixtureKind.On)
            return 1f - 0.03f * MathF.Sin(time * 2.1f + phase * 40f);

        var burst = MathF.Sin(time * 0.55f + phase * 17f) + MathF.Sin(time * 1.37f + phase * 7f) * 0.5f;
        if (burst < 0.6f)
            return 1f;

        var flickerStep = (int) MathF.Floor(time * 12f + phase * 100f);
        var flicker = NullspaceMaze.Hash(0x77, flickerStep, cell.X, cell.Y) % 100 / 100f;
        return flicker < 0.35f ? 0.03f : flicker < 0.5f ? 0.45f : 1f;
    }
}
