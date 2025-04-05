namespace Content.Goobstation.Server.Wizard.Teleport;

[RegisterComponent]
public sealed partial class WizardTeleportLocationComponent : Component
{
    [DataField]
    public string? Location;
}
