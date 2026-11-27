using System.Numerics;
using Content.Goobstation.Shared.Nullspace;

namespace Content.Goobstation.Client.Nullspace;

public sealed partial class NullspaceSystem
{
    private const float FlashlightIntensity = 45f;
    private const float OtherFlashlightRange = 24f;
    private const float BillboardRange = 30f;

    private readonly List<(float Distance, NullspaceSpot Spot)> _otherSpots = new();
    private readonly List<EntityUid> _staleTracks = new();

    private bool TrackClown(NullspaceClownTrack track, EntityUid map)
    {
        EntityUid? found = null;
        var query = EntityQueryEnumerator<NullspaceClownComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var clown, out var xform))
        {
            if (xform.MapUid != map)
                continue;

            found = uid;
            track.State = clown.State;
            track.Target = clown.Target;
            break;
        }

        if (found is not { } current)
        {
            track.Uid = null;
            return false;
        }

        var position = _transform.GetWorldPosition(current) * Cell;
        var arrived = track.Uid != current;
        if (arrived)
        {
            track.Uid = current;
            track.Position = position;
        }

        var moved = (position - track.Position).Length();
        track.Position = position;
        if (moved < Cell)
            track.Phase += moved * MathF.PI / NullspaceParams.ClownStride;

        return arrived;
    }

    private static (float Distance, bool CanSee) ClownSight(NullspaceClownTrack track, NullspaceMaze maze, Vector2 viewer)
    {
        if (track.Uid == null)
            return (float.PositiveInfinity, false);

        var distance = (track.Position - viewer).Length();
        var canSee = distance < NullspaceParams.ClownSightRange
            && maze.LineClear(track.Position.X / Cell, track.Position.Y / Cell, viewer.X / Cell, viewer.Y / Cell);
        return (distance, canSee);
    }

    private void FillView(
        NullspaceFrame frame,
        NullspaceMaze maze,
        Vector3 camera,
        Vector3 forward,
        Vector3 right,
        Vector3 up,
        float fov,
        float time)
    {
        frame.Maze = maze;
        frame.CameraPosition = camera;
        frame.CameraForward = forward;
        frame.CameraRight = right;
        frame.CameraUp = up;
        frame.Focal = 0.5f / MathF.Tan(MathHelper.DegreesToRadians(fov) * 0.5f);
        frame.Time = time;
    }

    private void FillActors(
        NullspaceFrame frame,
        NullspaceTracks tracks,
        EntityUid map,
        EntityUid viewer,
        Vector3 camera,
        Vector3 forward,
        Vector3 right,
        Vector3 up,
        Vector3 flashlight,
        float flashlightIntensity,
        float otherFlashlights,
        bool caught)
    {
        frame.Billboards.Clear();
        frame.Spots.Clear();
        frame.Spots.Add(new NullspaceSpot(camera + right * 0.18f - up * 0.16f, flashlight, flashlightIntensity));

        if (tracks.Clown.Uid is { } clown)
        {
            frame.Billboards.Add(caught
                ? new NullspaceBillboard(clown, camera + forward * 0.55f - up * 0.56f, right, up, Direction.South, true, true)
                : Billboard(clown, tracks.Clown.Position, tracks.Clown.Phase, camera, true));
        }

        _otherSpots.Clear();
        _staleTracks.Clear();
        _staleTracks.AddRange(tracks.Players.Keys);

        var query = EntityQueryEnumerator<NullspacePlayerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var player, out var xform))
        {
            if (uid == viewer || xform.MapUid != map)
                continue;

            _staleTracks.Remove(uid);
            var position = _transform.GetWorldPosition(xform) * Cell;
            var phase = 0f;
            if (tracks.Players.TryGetValue(uid, out var track))
            {
                var moved = (position - track.Position).Length();
                phase = track.Phase + (moved < Cell ? moved * MathF.PI / NullspaceParams.WalkStride : 0f);
            }

            tracks.Players[uid] = (position, phase);

            var distance = (new Vector2(camera.X, camera.Y) - position).Length();
            if (distance <= BillboardRange)
                frame.Billboards.Add(Billboard(uid, position, phase, camera, false));

            if (distance > OtherFlashlightRange || player.Frozen)
                continue;

            var yaw = WrapAngle((float) (_transform.GetWorldRotation(xform).Theta - Math.PI));
            var (theirForward, theirRight, theirUp, _, _) = Basis(yaw, player.Pitch, 0f);
            var eye = new Vector3(position.X, position.Y, NullspaceParams.Eye);
            _otherSpots.Add((distance, new NullspaceSpot(eye + theirRight * 0.18f - theirUp * 0.16f, theirForward, otherFlashlights)));
        }

        foreach (var stale in _staleTracks)
        {
            tracks.Players.Remove(stale);
        }

        _otherSpots.Sort(static (a, b) => a.Distance.CompareTo(b.Distance));
        for (var i = 0; i < _otherSpots.Count && frame.Spots.Count < NullspaceFrame.MaxSpots; i++)
        {
            frame.Spots.Add(_otherSpots[i].Spot);
        }
    }

    private NullspaceBillboard Billboard(EntityUid uid, Vector2 position, float phase, Vector3 camera, bool glows)
    {
        var toCamera = new Vector2(camera.X - position.X, camera.Y - position.Y);
        var normal = toCamera.LengthSquared() > 1e-6f ? Vector2.Normalize(toCamera) : Vector2.UnitY;
        var right = new Vector3(-normal.Y, normal.X, 0f);
        var sway = MathF.Sin(phase) * 0.05f;
        var center = new Vector3(position.X, position.Y, NullspaceParams.ClownHeight * 0.5f + MathF.Abs(MathF.Sin(phase)) * 0.06f);
        var facing = _transform.GetWorldRotation(uid) - new Angle(-toCamera) + Angle.FromDegrees(90);

        return new NullspaceBillboard(
            uid,
            center,
            right * MathF.Cos(sway) + Vector3.UnitZ * MathF.Sin(sway),
            Vector3.UnitZ * MathF.Cos(sway) - right * MathF.Sin(sway),
            facing.GetCardinalDir(),
            glows,
            false);
    }

    private static (Vector3 Forward, Vector3 Right, Vector3 Up, Vector3 Flat, Vector3 FlatRight) Basis(float yaw, float pitch, float roll)
    {
        var flat = new Vector3(-MathF.Sin(yaw), MathF.Cos(yaw), 0f);
        var flatRight = new Vector3(MathF.Cos(yaw), MathF.Sin(yaw), 0f);
        var forward = flat * MathF.Cos(pitch) + Vector3.UnitZ * MathF.Sin(pitch);
        var lookUp = Vector3.UnitZ * MathF.Cos(pitch) - flat * MathF.Sin(pitch);
        var right = flatRight * MathF.Cos(roll) + lookUp * MathF.Sin(roll);
        var up = lookUp * MathF.Cos(roll) - flatRight * MathF.Sin(roll);
        return (forward, right, up, flat, flatRight);
    }
}
