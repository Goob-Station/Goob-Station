using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.GPS;

[RegisterComponent, NetworkedComponent]
public sealed partial class SignalLocatorComponent : Component
{
    [DataField]
    public float UpdateFrequency = 1f;

    [ViewVariables]
    public float UpdateAccumulator;

    [ViewVariables]
    public List<LavalandSignal> Signals = new();

    [DataField]
    public string LocationName = "Unknown";

    [DataField]
    public GpsRefreshType RefreshType = GpsRefreshType.Manual;

    [DataField]
    public GpsRangeType RangeType = GpsRangeType.Medium;

    public float GetCurrentRange => RangeDict[RangeType];

    [DataField]
    public Dictionary<GpsRangeType, float> RangeDict = new()
    {
        { GpsRangeType.Low, 50f },
        { GpsRangeType.Medium, 200f },
        { GpsRangeType.High, float.MaxValue },
        { GpsRangeType.Max, float.MaxValue }, // But also between maps
    };
}

[Serializable, NetSerializable]
public record struct LavalandSignal(NetEntity Entity, string Name, Vector2i Position, MapId MapId)
{
    public NetEntity Entity = Entity;
    public string Name = Name;
    public Vector2i Position = Position;
    public MapId MapId = MapId;
}
