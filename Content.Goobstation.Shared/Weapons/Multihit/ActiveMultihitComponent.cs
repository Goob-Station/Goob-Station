namespace Content.Goobstation.Shared.Weapons.Multihit;

[RegisterComponent]
public sealed partial class ActiveMultihitComponent : Component
{
    [ViewVariables]
    public float DamageMultiplier = 1f;
}
