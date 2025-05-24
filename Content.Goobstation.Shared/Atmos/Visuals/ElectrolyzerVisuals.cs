using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Atmos.Visuals;

/// <summary>
///     Assmos - /tg/ gases
///     Used for the visualizer
/// </summary>
[Serializable, NetSerializable]
public enum ElectrolyzerVisualLayers : byte
{
    Main
}

[Serializable, NetSerializable]
public enum ElectrolyzerVisuals : byte
{
    State,
}

[Serializable, NetSerializable]
public enum ElectrolyzerState : byte
{
    Off,
    On,
}
