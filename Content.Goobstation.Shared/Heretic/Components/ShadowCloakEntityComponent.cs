using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowCloakEntityComponent : Component
{
    [DataField]
    public float Lifetime = 3.2f;

    [ViewVariables]
    public float? DeletionAccumulator;
}
