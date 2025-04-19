namespace Content.Goobstation.Shared.NTR;

[RegisterComponent]
public sealed partial class NtrBankAccountComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("balance")]
    public int Balance = 0;

    [DataField]
    public int TotalEarned = 0;
}
