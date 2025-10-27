namespace Content.Shared._Lavaland.Damage;

/// <summary>
/// This is used for modifying the melee damage a entyty does
/// </summary>
[RegisterComponent]
public sealed partial class ModifyMeleeDamageComponent : Component
{
    [DataField]
    public float Modifier = 1.25f;
}
