using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._ES.Camera;

/// <summary>
///     Marks an entity which is actively screenshaking because of a screenshake command being given.
/// </summary>
/// <remarks>
///     This doesn't mark an entity which *can* screenshake--all entities can, by default, as long as a client is controlling them.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ESScreenshakeComponent : Component
{
    /// <summary>
    ///     A set of screenshake commands which this entity is currently processing.
    ///     Trauma is 0..1, with 0 being no shake and 1 being maximum shake.
    /// </summary>
    /// <remarks>
    ///     This is a set, because order doesn't matter, and we don't want to accidentally readd the same command twice.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public HashSet<ESScreenshakeCommand> Commands = new();

    public override bool SendOnlyToOwner => true;
}

/// <summary>
///     Represents a single screenshake command. These are stored and networked on <see cref="ESScreenshakeComponent"/>,
///     and the client that controls that entity will use the trauma values in each command, and their start time,
///     to calculate multipliers on the current eye offset & rotation modifiers.
/// </summary>
/// <param name="Translational">Parameters of translational screenshake (offset-based)</param>
/// <param name="Rotational">Parameters of rotational screenshake (rotation-based)</param>
/// <param name="Start">Time this screenshake command was added.</param>
/// <param name="CalculatedEnd">The end time for this command, calculated from trauma, decay rate, and start time.</param>
[DataRecord, Serializable, NetSerializable]
public partial record ESScreenshakeCommand(ESScreenshakeParameters? Translational, ESScreenshakeParameters? Rotational, TimeSpan Start, TimeSpan CalculatedEnd);

/// <summary>
///     Represents the parameters of a single instance of screenshake, which may apply to translational or rotational shake.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public partial record ESScreenshakeParameters()
{
    /// <summary>
    ///     Strength of the shake.
    /// </summary>
    [DataField(required: true)]
    public float Trauma = 0f;

    /// <summary>
    ///     How fast the shake decays.
    /// </summary>
    [DataField]
    public float DecayRate = 1.2f;

    /// <summary>
    ///     How frantically the shake oscillates.
    /// </summary>
    [DataField]
    public float Frequency = 0.01f;

    public ESScreenshakeParameters(float trauma, float decayRate = 1.2f, float frequency = 0.01f) : this()
    {
        Trauma = trauma;
        DecayRate = decayRate;
        Frequency = frequency;
    }
};
