namespace Content.Server._BloodCult.Touchspell;

[RegisterComponent]
public sealed partial class FuuMajinComponent : Component
{
    [DataField] public float StunDuration = 16f;
    [DataField] public float MuteDuration = 12f;
    [DataField] public float SpeechDuration = 30f;
}
