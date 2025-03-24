namespace Content.Server._Goobstation.Wizard.Teleport;

[RegisterComponent]
public sealed partial class WizardTeleportLocationComponent : Component
{
    [DataField]
    public string? Location;
}
