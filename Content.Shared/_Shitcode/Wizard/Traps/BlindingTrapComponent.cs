namespace Content.Shared._Goobstation.Wizard.Traps;

[RegisterComponent]
public sealed partial class BlindingTrapComponent : Component
{
    [DataField]
    public TimeSpan BlindDuration = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan BlurDuration = TimeSpan.FromSeconds(30);
}
