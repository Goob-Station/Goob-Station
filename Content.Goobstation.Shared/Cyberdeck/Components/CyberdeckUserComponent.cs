using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cyberdeck.Components;

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
    public ProtoId<AlertPrototype> AlertId = "CyberdeckCharges";

    [DataField]
    public FixedPoint2 CyberVisionAbilityCost = 2;

    [DataField]
    public EntProtoId ProjectionEntityId = "CyberdeckProjection";

    [DataField]
    public EntProtoId HackActionId = "ActionCyberdeckHack";

    [DataField]
    public EntProtoId VisionActionId = "ActionCyberdeckVision";

    [DataField]
    public EntProtoId ReturnActionId = "ActionCyberdeckVisionReturn";
}
