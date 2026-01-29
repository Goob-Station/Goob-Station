using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.InternalResources.Data;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
/// Component used to mark changelings that use biomass. Typically only via Awakened Instinct.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangelingBiomassComponent : Component
{
    /// <summary>
    /// The internal resource prototype to be added.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ResourceProto = "ChangelingBiomass";

    /// <summary>
    /// The InternalResourcesData of the prototype.
    /// </summary>
    [DataField]
    public InternalResourcesData? ResourceData;

    /// <summary>
    /// The ProtoID of the changeling chemicals.
    /// </summary>
    [DataField]
    public ProtoId<InternalResourcesPrototype> ChemResourceType = "ChangelingChemicals";

    /// <summary>
    /// The amount that the changeling's chemical regeneration multiplier will increase by
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChemicalBoost = 0.25f;

    // first threshold
    [DataField]
    public LocId FirstWarnPopup = "changeling-biomass-warn-first";

    // second threshold
    [DataField]
    public LocId SecondWarnPopup = "changeling-biomass-warn-second";

    [DataField, AutoNetworkedField]
    public TimeSpan SecondWarnStun = TimeSpan.FromSeconds(1);

    // third threshold
    [DataField]
    public LocId ThirdWarnPopup = "changeling-biomass-warn-third";

    [DataField, AutoNetworkedField]
    public TimeSpan ThirdWarnStun = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public FixedPoint2 BloodCoughAmount = 2f;

    [DataField]
    public ProtoId<EmotePrototype> CoughEmote = "Cough";

    // final threshold (death)
    [DataField]
    public LocId NoBiomassPopup = "changeling-biomass-warn-death";
}
