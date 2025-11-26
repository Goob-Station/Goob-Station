using Content.Shared.Destructible.Thresholds;
using Content.Shared.Weather;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.Weather;

/// <summary>
/// Makes weather randomly happen every so often.
/// </summary>
[RegisterComponent, Access(typeof(WeatherSchedulerSystem))]
[AutoGenerateComponentPause]
public sealed partial class WeatherSchedulerComponent : Component
{
    /// <summary>
    /// Weather stages to schedule.
    /// </summary>
    [DataField(required: true)]
    public List<WeatherStage> Stages = new();

    /// <summary>
    /// The index of <see cref="Stages"/> to use next, wraps back to the start.
    /// </summary>
    [DataField]
    public int Stage;

    /// <summary>
    /// When to go to the next step of the schedule.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate;
}

/// <summary>
/// A stage in a weather schedule.
/// </summary>
[Serializable, DataDefinition]
public partial struct WeatherStage
{
    /// <summary>
    /// A range of how long the stage can last for, in seconds.
    /// </summary>
    [DataField(required: true)]
    public MinMax Duration = new(0, 0);

    /// <summary>
    /// The weather prototype to add, or null for clear weather.
    /// </summary>
    [DataField]
    public ProtoId<WeatherPrototype>? Weather;

    /// <summary>
    /// Alert message to send in chat for players on the map when it starts.
    /// </summary>
    [DataField]
    public LocId? Message;
}
