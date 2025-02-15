namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantBossComponent : MegafaunaComponent
{
    /// <summary>
    /// Amount of time for one damaging tile to charge up and deal the damage to anyone above it.
    /// </summary>
    public const float TileDamageDelay = 0.6f;

    // FYI don't use these delays as they are, better use the GetDelay() system method since it accounts for anger.

    /// <summary>
    ///     Gets calculated automatically in the <see cref="HierophantSystem"/>.
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
    public float InterActionDelay = 3 * TileDamageDelay * 1000f;

    [DataField]
    public float MajorAttackCooldown = 12f * TileDamageDelay;

    [ViewVariables]
    public float MajorAttackTimer = 12f * TileDamageDelay;

    [DataField]
    public float AttackCooldown = 8f * TileDamageDelay;

    [ViewVariables]
    public float AttackTimer = 4f * TileDamageDelay;

    [DataField]
    public float MinAttackCooldown = 3f * TileDamageDelay;

    [DataField]
    public float MinMajorAttackCooldown = 6f * TileDamageDelay;

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
    public EntityUid? ConnectedFieldGenerator;
}
