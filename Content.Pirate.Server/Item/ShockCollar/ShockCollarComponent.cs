namespace Content.Pirate.Server.ShockCollar;

[RegisterComponent]
public sealed partial class ShockCollarComponent : Component
{
    [DataField]
    public int ShockDamage = 1;

    [DataField]
    public TimeSpan ShockTime = TimeSpan.FromSeconds(2);
}

