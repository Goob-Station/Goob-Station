using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LastRefugeActionComponent : Component
{
    [DataField]
    public float OtherMindsCheckRange = 5f;
}
