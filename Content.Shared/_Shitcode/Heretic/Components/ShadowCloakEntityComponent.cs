using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowCloakEntityComponent : Component
{
    [DataField]
    public float Lifetime = 3.2f;

    [ViewVariables]
    public float? DeletionAccumulator;
}
