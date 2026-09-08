using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hamon;

/// <summary>
/// Your next line is...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PredictNextLineComponent : Component
{
    [DataField]
    public string Message = "You're next line is...";

    [DataField]
    public EntityUid? Target;

    [DataField]
    public EntProtoId Action = "ActionPredictNextLine";
}
