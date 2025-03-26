using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Chat;

[RegisterComponent, NetworkedComponent]
public sealed partial class TheSwapComponent : Component
{
    [DataField]
    public EntProtoId? SpellAction = "ActionSwapIII";

    [DataField]
    public EntProtoId? Implant = "SyndicateJaunterImplant";

    [DataField]
    public string? Name = "THE SWAP";
}
