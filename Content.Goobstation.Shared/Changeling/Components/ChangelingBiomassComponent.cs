using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
/// Component used to mark changelings that use biomass. Typically only via Awakened Instinct.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class ChangelingBiomassComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> AlertId = "ChangelingBiomass";

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan UpdateTimer = default!;

    /// <summary>
    /// Delay between update cycles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(30); // will last 50 minutes give or take

    /// <summary>
    /// Current level of biomass the changeling currently has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Biomass = 100.0f;

    /// <summary>
    /// Maximum level of biomass the changeling can have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxBiomass = 100.0f;

    /// <summary>
    /// The biomass drain per cycle.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DrainAmount = 1f;

    /// <summary>
    /// The amount that the changeling's chemical regeneration multiplier will increase by
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChemicalBoost = 0.25f;

    // first warning
    [DataField, AutoNetworkedField]
    public bool FirstWarnReached;

    [DataField, AutoNetworkedField]
    public float FirstWarnThreshold;

    [DataField]
    public LocId FirstWarnPopup = "changeling-biomass-warn-first";

    // second warning
    [DataField, AutoNetworkedField]
    public bool SecondWarnReached;

    [DataField, AutoNetworkedField]
    public float SecondWarnThreshold;

    [DataField]
    public LocId SecondWarnPopup = "changeling-biomass-warn-second";

    [DataField, AutoNetworkedField]
    public TimeSpan SecondWarnStun = TimeSpan.FromSeconds(1);

    // third warning
    [DataField, AutoNetworkedField]
    public bool ThirdWarnReached;

    [DataField, AutoNetworkedField]
    public float ThirdWarnThreshold;

    [DataField]
    public LocId ThirdWarnPopup = "changeling-biomass-warn-third";

    [DataField, AutoNetworkedField]
    public TimeSpan ThirdWarnStun = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public FixedPoint2 BloodCoughAmount = 2f;

    [DataField]
    public ProtoId<EmotePrototype> CoughEmote = "Cough";

    [DataField]
    public LocId NoBiomassPopup = "changeling-biomass-warn-death";
}
