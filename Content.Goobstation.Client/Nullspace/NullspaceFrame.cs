using System.Numerics;
using Content.Goobstation.Shared.Nullspace;

namespace Content.Goobstation.Client.Nullspace;

public readonly record struct NullspaceFixtureVisual(Vector2i Cell, float Level, bool AlongX);

public readonly record struct NullspaceBillboard(
    EntityUid Entity,
    Vector3 Center,
    Vector3 Right,
    Vector3 Up,
    Direction Direction,
    bool Glows,
    bool InFace);

public readonly record struct NullspaceSpot(Vector3 Position, Vector3 Direction, float Intensity);

public sealed class NullspaceFrame
{
    public const int MaxSpots = 4;

    public NullspaceMaze? Maze;

    public Vector3 CameraPosition;
    public Vector3 CameraForward = Vector3.UnitY;
    public Vector3 CameraRight = Vector3.UnitX;
    public Vector3 CameraUp = Vector3.UnitZ;
    public float Focal = 0.69f;

    public readonly Vector4[] Lamps = new Vector4[4];
    public readonly List<NullspaceSpot> Spots = new();
    public readonly List<NullspaceFixtureVisual> Fixtures = new();
    public readonly List<NullspaceBillboard> Billboards = new();

    public float Time;
    public float Fear;
    public float Exhaustion;
    public float Flash;
    public float FadeIn;
}

public sealed class NullspaceTracks
{
    public readonly NullspaceClownTrack Clown = new();
    public readonly Dictionary<EntityUid, (System.Numerics.Vector2 Position, float Phase)> Players = new();
}
