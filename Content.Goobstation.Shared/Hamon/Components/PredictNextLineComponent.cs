using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hamon.Components;

/// <summary>
/// Your next line is...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PredictNextLineComponent : Component
{
    [DataField]
    public ProtoId<DatasetPrototype> Messages = "HamonPredictLines";

    [DataField]
    public EntityUid? Target;

    [DataField]
    public EntProtoId Action = "ActionPredictNextLine";
}
