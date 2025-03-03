using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.GPS;

[Serializable, NetSerializable]
public sealed class GpsRefreshMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class GpsRefreshRangeMessage(GpsRangeType rangeType) : BoundUserInterfaceMessage
{
    public readonly GpsRangeType RangeType = rangeType;
}

[Serializable, NetSerializable]
public sealed class GpsRefreshModeMessage(GpsRefreshType refreshType) : BoundUserInterfaceMessage
{
    public readonly GpsRefreshType RefreshType = refreshType;
}

[Serializable, NetSerializable]
public sealed class GpsSignalLocatorState(List<LavalandSignal> signals, (Vector2i, string) worldPos) : BoundUserInterfaceState
{
    public readonly List<LavalandSignal> Signals = signals;
    public readonly (Vector2i, string) LocatorPosition = worldPos;
}


[Serializable, NetSerializable]
public enum GpsRefreshType
{
    Auto,
    Manual,
}

[Serializable, NetSerializable]
public enum GpsRangeType
{
    Low, // Low distance
    Medium, // Higher distance
    High, // All map
    Max, // Also different maps
}

[Serializable, NetSerializable]
public enum SignalLocatorUiKey
{
    Key,
}
