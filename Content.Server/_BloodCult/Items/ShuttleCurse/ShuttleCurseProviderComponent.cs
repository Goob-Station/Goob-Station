namespace Content.Server.WhiteDream.BloodCult.Items.ShuttleCurse;

[RegisterComponent]
public sealed partial class ShuttleCurseProviderComponent : Component
{
    [DataField]
    public int MaxUses = 3;

    [ViewVariables(VVAccess.ReadOnly)]
    public int CurrentUses = 0;
}
