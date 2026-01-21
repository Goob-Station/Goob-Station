using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class GhoulDeconvertComponent : Component
{
    [DataField]
    public float Delay = 30f;
}
