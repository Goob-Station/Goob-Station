using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CosmosPassiveComponent : Component
{
    [DataField]
    public float StaminaHeal = -15f;
}
