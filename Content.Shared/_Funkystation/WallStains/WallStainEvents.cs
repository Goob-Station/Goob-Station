using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.WallStains;

[Serializable, NetSerializable]
public enum WallStainVisuals : byte
{
    Color,
    State
}

[Serializable, NetSerializable]
public sealed partial class CleanWallStainDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class PourOnWallDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class CleanWallStainsEvent(bool transformToWater = false) : EntityEventArgs
{
    public bool TransformToWater = transformToWater;
}

[ByRefEvent]
public readonly record struct SplashOnWallEvent(EntityCoordinates Coordinates, Solution Solution);
