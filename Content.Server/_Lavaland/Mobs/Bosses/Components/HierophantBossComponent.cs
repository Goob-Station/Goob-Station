namespace Content.Server._Lavaland.Mobs.Bosses.Components;

[RegisterComponent]
public sealed partial class HierophantBossComponent : MegafaunaComponent
{
    // FYI don't use these delays as they are, better use the GetDelay() system method since it accounts for anger.

    /// <summary>
    ///     Gets calculated automatically in the <see cref="HierophantBehaviorSystem"/>.
    ///     Is responsive for how fast the hierophant attacks.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public float Anger = 0f;
    [ViewVariables(VVAccess.ReadOnly)] public float InterActionDelay = .5f;

    [DataField] public float MajorAttackCooldown = 8f;
    public float MajorAttackTimer = 8f;

    [DataField] public float AttackCooldown = 4f;
    public float AttackTimer = 4f;

    [DataField] public float MeleeReactionCooldown = 10f;
    public float MeleeReactionTimer = 10f;
    public bool Meleed = false;

    /// <summary>
    ///     Spawns an AoE attack if being melee'd.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public bool ReactOnMelee = true;

    [ViewVariables(VVAccess.ReadOnly)] public bool IsAttacking = false;
}
