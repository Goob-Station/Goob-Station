namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantBossComponent : MegafaunaComponent
{
    // FYI don't use these delays as they are, better use the GetDelay() system method since it accounts for anger.

    /// <summary>
    ///     Gets calculated automatically in the <see cref="HierophantBehaviorSystem"/>.
    ///     Is responsive for how fast the hierophant attacks.
    /// </summary>
    [ViewVariables]
    public float Anger = 0f;

    [ViewVariables]
    public float InterActionDelay = 0.5f;

    [DataField]
    public float MajorAttackCooldown = 8f;

    [ViewVariables]
    public float MajorAttackTimer = 8f;

    [DataField]
    public float AttackCooldown = 4f;

    [ViewVariables]
    public float AttackTimer = 4f;

    [DataField]
    public float MeleeReactionCooldown = 10f;

    [ViewVariables]
    public float MeleeReactionTimer = 10f;

    [ViewVariables]
    public bool Meleed = false;

    /// <summary>
    ///     Spawns an AoE attack if being melee'd.
    /// </summary>
    [ViewVariables]
    public bool ReactOnMelee = true;

    [ViewVariables]
    public bool IsAttacking = false;
}
