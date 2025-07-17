namespace Content.Goobstation.Shared.SlaughterDemon.Objectives;

[RegisterComponent]
public sealed partial class SlaughterKillTheWizardConditionComponent : Component
{
    [DataField]
    public string? Title;

    [DataField]
    public string? Description;
}
