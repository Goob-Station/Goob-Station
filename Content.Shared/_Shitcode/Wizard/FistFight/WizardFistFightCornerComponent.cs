namespace Content.Shared._Shitcode.Wizard.FistFight;

[RegisterComponent]
public sealed partial class WizardFistFightCornerComponent : Component
{
    [DataField(required: true)]
    public FistFightCorner Corner;
}
