using Robust.Shared.GameStates;

namespace Content.Shared.Heretic.Effects;

[RegisterComponent, NetworkedComponent]
public sealed partial class NoClipComponent : Component
{
    [DataField]
    public LocId? ExamineLoc = "crucible-soul-effect-examine-message";
}
