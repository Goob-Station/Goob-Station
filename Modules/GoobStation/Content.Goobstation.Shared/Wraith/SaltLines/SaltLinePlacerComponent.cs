using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.SaltLines;

[RegisterComponent, NetworkedComponent]
public sealed partial class SaltLinePlacerComponent : Component
{
    [DataField]
    public EntProtoId SaltLine = "SaltLine";
}
