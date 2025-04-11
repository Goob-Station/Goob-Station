using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Administration.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SignOnHitComponent : Component
{
    [DataField]
    public string SignSprite = "Objects/Misc/killsign.rsi";
}