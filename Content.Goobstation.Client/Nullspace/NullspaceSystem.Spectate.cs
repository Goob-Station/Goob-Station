using System.Numerics;
using Content.Client.Administration.UI.AdminCamera;
using Content.Goobstation.Shared.Nullspace;

namespace Content.Goobstation.Client.Nullspace;

public sealed partial class NullspaceSystem
{
    [SubscribeLocalEvent]
    private void OnGetAdminCameraView(Entity<NullspacePlayerComponent> ent, ref GetAdminCameraViewEvent args)
    {
        args.View = new NullspaceSpectatorControl { Target = ent.Owner };
    }

    public NullspaceRenderer CreateRenderer()
    {
        return new NullspaceRenderer(_clyde, EntityManager, _prototypes, _resources, _tiles);
    }

    public void FillSpectatorFrame(NullspaceFrame frame, EntityUid target, NullspaceTracks tracks)
    {
        frame.Maze = null;
        if (!TryComp<NullspacePlayerComponent>(target, out var player) || !TryGetMap(target, out var map))
            return;

        var maze = GetMaze(map.Comp);
        var time = (float) (_timing.RealTime.TotalSeconds % 3600.0);
        var position = _transform.GetWorldPosition(target) * Cell;
        var yaw = WrapAngle((float) (_transform.GetWorldRotation(target).Theta - Math.PI));
        var (forward, right, up, _, _) = Basis(yaw, player.Pitch, 0f);
        var camera = new Vector3(position.X, position.Y, NullspaceParams.Eye);

        var track = tracks.Clown;
        TrackClown(track, map);
        var (distance, canSee) = ClownSight(track, maze, position);

        var caught = player.Frozen;
        var fear = caught ? 1f : FearTarget(track, target, distance, canSee);

        FillView(frame, maze, camera, forward, right, up, BaseFov + (player.Sprinting ? 4f : 0f) - fear * 8f, time);
        frame.Fear = fear;
        frame.Exhaustion = Exhaustion(player);
        frame.Flash = 0f;
        frame.FadeIn = 1f;

        CollectLights(frame, map.Comp, maze, position, time, track.Uid != null ? track.Position : null, false);
        var flashlight = Flashlight(NullspaceParams.Darkness(GameTime(map.Comp, _timing.CurTime)));
        FillActors(frame, tracks, map, target, camera, forward, right, up, forward, flashlight, flashlight, caught);
    }
}
