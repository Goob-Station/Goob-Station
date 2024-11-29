namespace Content.Server.Heretic.Components;

[RegisterComponent]
public sealed partial class MansusInfusedComponent : Component
{
    [DataField] public float MaxCharges = 5f;
    [ViewVariables(VVAccess.ReadWrite)] public float AvailableCharges = 5f;
}
