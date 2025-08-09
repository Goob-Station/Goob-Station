using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RealignmentComponent : Component
{
    [DataField]
    public string StaminaRegenKey = "Realignment";
}
