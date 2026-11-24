namespace Content.Shared.Zombies;

[AutoGenerateComponentPause]
public sealed partial class PendingZombieComponent
{
    [DataField]
    public TimeSpan SymptomInterval = TimeSpan.FromSeconds(15);

    [DataField, AutoPausedField]
    public TimeSpan NextSymptom;
}
