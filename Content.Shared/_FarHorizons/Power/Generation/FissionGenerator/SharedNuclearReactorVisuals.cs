using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

#region Reactor Caps
/// <summary>
/// Appearance keys for the reactor caps.
/// </summary>
[Serializable, NetSerializable]
public enum ReactorCapVisuals
{
    Sprite
}

/// <summary>
/// Visual sprite layers for the reactor cap.
/// </summary>
[Serializable, NetSerializable]
public enum ReactorCapVisualLayers
{
    Sprite
}

/// <summary>
/// Reactor cap sprites.
/// </summary>
[Serializable, NetSerializable]
public enum ReactorCaps
{
    Base,

    Control,
    ControlM1,
    ControlM2,
    ControlM3,
    ControlM4,

    Fuel,
    FuelM1,
    FuelM2, 
    FuelM3,
    FuelM4,

    Gas,
    GasM1,
    GasM2,
    GasM3,
    GasM4,

    Heat,
    HeatM1,
    HeatM2,
    HeatM3,
    HeatM4,
}
#endregion

#region Reactor
/// <summary>
/// Appearance keys for the reactor.
/// </summary>
[Serializable, NetSerializable]
public enum ReactorVisuals
{
    Sprite,
    Status,
    Input,
    Output,
    Lights,
    Smoke,
    Fire,
}

/// <summary>
/// Visual sprite layers for the reactor.
/// </summary>
[Serializable, NetSerializable]
public enum ReactorVisualLayers
{
    Sprite,
    Status,
    Input,
    Output,
    Lights,
    Smoke,
    Fire,
}

/// <summary>
/// Reactor sprites.
/// </summary>
[Serializable, NetSerializable]
public enum Reactors
{
    Normal,
    Melted,
}

/// <summary>
/// Status screens.
/// </summary>
[Serializable, NetSerializable]
public enum ReactorStatusLights
{
    Off,
    Active,
    Overheat,
    Meltdown,
    Boom,
}

/// <summary>
/// Warning lights settings.
/// </summary>
[Serializable, NetSerializable]
public enum ReactorWarningLights
{
    LightsOff,
    LightsWarning,
    LightsMeltdown,
}
#endregion