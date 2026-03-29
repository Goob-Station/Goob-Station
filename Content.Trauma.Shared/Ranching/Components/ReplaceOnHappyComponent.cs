using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReplaceOnHappyComponent : Component
{
    [DataField]
    public float HappinessRequired = 777f;

    [DataField]
    public EntProtoId Entity;
}
