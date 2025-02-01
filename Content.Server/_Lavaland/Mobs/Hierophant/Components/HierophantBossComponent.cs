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
    public float CurrentAnger = 1f;

    /// <summary>
    /// Minimal amount of anger that Hierophant can have.
    /// Tends to 2 when health tends to 0.
    /// </summary>
    [DataField]
    public float MinAnger = 1f;

    [DataField]
    public float MaxAnger = 3f;

    [DataField]
    public float InterActionDelay = 1.5f * 1000f;

    [DataField]
    public float MajorAttackCooldown = 5f;

    [ViewVariables]
    public float MajorAttackTimer = 5f;

    [DataField]
    public float AttackCooldown = 4f;

    [ViewVariables]
    public float AttackTimer = 2f;

    [DataField]
    public float MinAttackCooldown = 2f;

    [DataField]
    public float MinMajorAttackCooldown = 3f;

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

    [ViewVariables]
    public Entity<HierophantFieldGeneratorComponent>? ConnectedFieldGenerator;
}
