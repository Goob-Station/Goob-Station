using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticCosmicRuneActionComponent : Component
{
    [DataField]
    public EntityUid? FirstRune;

    [DataField]
    public EntityUid? SecondRune;
}
