// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weather;

[RegisterComponent, NetworkedComponent]
public sealed partial class WeatherComponent : Component
{
    /// <summary>
    /// Currently running weathers
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<WeatherPrototype>, WeatherData> Weather = new();

    /// <summary>
    /// DeltaV: How long to wait between updating weather effects.
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// DeltaV: When to next update weather effects (damage).
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    public static readonly TimeSpan StartupTime = TimeSpan.FromSeconds(15);
    public static readonly TimeSpan ShutdownTime = TimeSpan.FromSeconds(15);
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class WeatherData
{
    // Client audio stream.
    [NonSerialized]
    public EntityUid? Stream;

    /// <summary>
    /// When the weather started if relevant.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))] //TODO: Remove Custom serializer
    public TimeSpan StartTime = TimeSpan.Zero;

    /// <summary>
    /// When the applied weather will end.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))] //TODO: Remove Custom serializer
    public TimeSpan? EndTime;

    [ViewVariables]
    public TimeSpan Duration => EndTime == null ? TimeSpan.MaxValue : EndTime.Value - StartTime;

    [DataField]
    public WeatherState State = WeatherState.Invalid;
}

public enum WeatherState : byte
{
    Invalid = 0,
    Starting,
    Running,
    Ending,
}