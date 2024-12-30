using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Bingle;

[RegisterComponent]
public sealed partial class BingleComponent : Component
{
    [DataField]
    public bool Upgraded = false;
}
