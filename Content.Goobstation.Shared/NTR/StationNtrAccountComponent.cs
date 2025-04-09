using Content.Goobstation.Shared.NTR;

namespace Content.Goobstation.Shared.NTR;

[RegisterComponent]
public sealed partial class StationNtrAccountComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("balance")]
    public int Balance = 0;
}
