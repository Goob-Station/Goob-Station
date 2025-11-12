using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FleshMimickedComponent : Component
{
    [DataField]
    public List<EntityUid> FleshMimics = new();
}
