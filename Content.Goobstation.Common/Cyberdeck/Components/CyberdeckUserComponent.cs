using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Cyberdeck.Components;

/// <remarks>
/// This won't work if assigned by hand. Use CyberDeckSystem instead.
/// </remarks>>
[RegisterComponent]
public sealed partial class CyberdeckUserComponent : Component
{
    /// <summary>
    /// Entity that provides cyberdeck abilities to this entity.
    /// Used mostly for counting charges, and if null charges will just be infinite.
    /// </summary>
    [ViewVariables]
    public EntityUid? ProviderEntity;

    [ViewVariables]
    public EntityUid? ProjectionEntity;

    [ViewVariables]
    public EntityUid? HackAction;

    [ViewVariables]
    public EntityUid? VisionAction;

    [ViewVariables]
    public EntityUid? ReturnAction;

    [DataField]
    public string AlertId = "CyberdeckCharges";

    [DataField]
    public int CyberVisionAbilityCost = 5;

    [DataField]
    public EntProtoId ProjectionEntityId = "CyberdeckProjection";

    [DataField]
    public EntProtoId HackActionId = "ActionCyberdeckHack";

    [DataField]
    public EntProtoId VisionActionId = "ActionCyberdeckVision";

    [DataField]
    public EntProtoId ReturnActionId = "ActionCyberdeckVisionReturn";
}
