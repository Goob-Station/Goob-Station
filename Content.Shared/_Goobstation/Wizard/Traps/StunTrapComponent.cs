namespace Content.Shared._Goobstation.Wizard.Traps;

[RegisterComponent]
public sealed partial class StunTrapComponent : Component
{
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(10);

    [DataField]
    public int Damage = 30;
}
