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

    [DataField] public float MajorAttackCooldown = 6f;

    [DataField] public float AttackCooldown = 4f;

    [DataField] public float MeleeReactionCooldown = 5f;

    /// <summary>
    ///     Spawns an AoE attack if being melee'd.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public bool ReactOnMelee = true;
}
