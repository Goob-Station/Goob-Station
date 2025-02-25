namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class ThrownLightningComponent : Component
{
    [DataField]
    public float StaminaDamage = 60f;

    [DataField]
    public LocId? Speech = "action-speech-spell-thrown-lightning";
}
