using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Cyberdeck.Components;

/// <remarks>
/// This won't work if assigned by hand. Use CyberDeckSystem instead.
/// </remarks>>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberdeckUserComponent : Component
{
    /// <summary>
    /// Entity that provides cyberdeck abilities to this entity.
    /// Used mostly for counting charges, and if null charges will just be infinite.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ProviderEntity;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ProjectionEntity;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? HackAction;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? VisionAction;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ReturnAction;

    [DataField]
    public string AlertId = "CyberdeckCharges";

    [DataField]
    public int CyberVisionAbilityCost = 6;

    [DataField]
    public EntProtoId ProjectionEntityId = "CyberdeckProjection";

    [DataField]
    public EntProtoId HackActionId = "ActionCyberdeckHack";

    [DataField]
    public EntProtoId VisionActionId = "ActionCyberdeckVision";

    [DataField]
    public EntProtoId ReturnActionId = "ActionCyberdeckVisionReturn";
}
