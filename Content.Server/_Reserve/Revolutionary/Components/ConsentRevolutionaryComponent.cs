using Content.Server._Reserve.Revolutionary.UI;

namespace Content.Server._Reserve.Revolutionary.Components;

[RegisterComponent]
public sealed partial class ConsentRevolutionaryComponent : Component
{
    /// <summary>
    /// Other member of convert request. If this is null, then entity is not in convert requestion.
    /// </summary>
    [DataField] public EntityUid? OtherMember;

    /// <summary>
    /// If entity is converter. If not, it is requested to be converted
    /// </summary>
    [DataField] public bool IsConverter = false;

    /// <summary>
    /// Window for consent convert.
    /// </summary>
    public ConsentRequestedEui? Window;

    /// <summary>
    /// Last time when entity was requested to be revolutionary
    /// </summary>
    [DataField] public TimeSpan? RequestStartTime;

    /// <summary>
    /// Time given to give response to request
    /// </summary>
    [DataField] public TimeSpan ResponseTime = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Time in which entity is can't be converted
    /// </summary>
    [DataField] public TimeSpan RequestBlockTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Time in which entity cannot request conversion
    /// </summary>
    [DataField] public TimeSpan ConversionBlockTime = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Distance in which request works
    /// </summary>
    [DataField] public float MaxDistance = 3f;
}
