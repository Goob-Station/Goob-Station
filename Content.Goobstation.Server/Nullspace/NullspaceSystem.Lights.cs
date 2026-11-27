using System.Numerics;
using Content.Goobstation.Shared.Nullspace;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Nullspace;

public sealed partial class NullspaceSystem
{
    private readonly List<Vector2i> _calmed = new();

    private void UpdateLights(Entity<NullspaceInstanceComponent, NullspaceMapComponent> ent, NullspaceMaze maze, float darkness)
    {
        if (ent.Comp1.Clown is not { } clown || TerminatingOrDeleted(clown))
            return;

        var clownPos = _transform.GetWorldPosition(clown) * Cell;
        var disturbed = ent.Comp1.DisturbedLights;

        _calmed.Clear();
        foreach (var cell in disturbed)
        {
            if ((CellCenter(cell) - clownPos).Length() > NullspaceParams.LightCalmRange)
                _calmed.Add(cell);
        }

        foreach (var cell in _calmed)
        {
            disturbed.Remove(cell);
        }

        var clownCell = CellOf(clownPos / Cell);
        var range = (int) MathF.Ceiling(NullspaceParams.LightKillRange / Cell);
        for (var dy = -range; dy <= range; dy++)
        {
            for (var dx = -range; dx <= range; dx++)
            {
                var cell = clownCell + new Vector2i(dx, dy);
                if (maze.Fixture(cell.X, cell.Y, darkness) is not { } kind
                    || kind == NullspaceFixtureKind.Dead
                    || ent.Comp2.DeadLights.Contains(cell))
                {
                    continue;
                }

                var center = CellCenter(cell);
                if (!NearAnyPlayer(ent.Comp1, center)
                    || (center - clownPos).Length() >= NullspaceParams.LightKillRange
                    || !disturbed.Add(cell)
                    || !_random.Prob(NullspaceParams.LightKillChance))
                {
                    continue;
                }

                ent.Comp2.DeadLights.Add(cell);
                Dirty(ent.Owner, ent.Comp2);

                var sound = _audio.PlayPvs(NullspaceAudio.LightOff,
                    ToCoordinates(ent.Owner, center),
                    NullspaceAudio.Positional(0.9f, 1f, 1.5f, 1f, 40f));
                Unoccluded(sound);
                ApplyReverb(sound);
            }
        }
    }

    private bool NearAnyPlayer(NullspaceInstanceComponent instance, Vector2 meters)
    {
        foreach (var body in instance.Players)
        {
            if (!TerminatingOrDeleted(body)
                && (_transform.GetWorldPosition(body) * Cell - meters).Length() <= NullspaceParams.LightActiveRange)
            {
                return true;
            }
        }

        return false;
    }
}
