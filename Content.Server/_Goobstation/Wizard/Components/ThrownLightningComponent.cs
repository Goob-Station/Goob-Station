namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class ThrownLightningComponent : Component
{
    [DataField]
    public float StaminaDamage = 80f;

    [DataField]
    public LocId? Speech = "action-speech-spell-thrown-lightning";
}
