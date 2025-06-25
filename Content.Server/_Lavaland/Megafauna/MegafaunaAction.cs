using JetBrains.Annotations;

namespace Content.Server._Lavaland.Megafauna;

/// <summary>
/// Seals a method to be invoked by some megafauna AI.
/// </summary>
/// <remarks>
/// If you want to make this action reusable, just make sure that at all steps
/// it doesn't require any specific components, and specify everything required
/// for the attack in DataFields.
///
/// In the future maybe we could even create a mega-ultra-boss for hehe-hahas
/// that casts all possible attacks at once on the player.
///
/// Pwetty pweeease, keep your code reusable so someone can make this funny idea in the future!!!
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class MegafaunaAction
{
    /// <summary>
    /// Current weight for this action to be picked.
    /// </summary>
    [DataField] public float Weight = 1f;

    public virtual string Name => GetType().Name;

    /// <returns>
    /// Duration of this attack in seconds
    /// </returns>
    public abstract float Invoke(MegafaunaAttackBaseArgs args);
}

public record MegafaunaAttackBaseArgs
{
    public EntityUid BossEntity;

    public IEntityManager EntityManager = default!;

    public MegafaunaAttackBaseArgs(EntityUid targetEntity, IEntityManager entityManager)
    {
        BossEntity = targetEntity;
        EntityManager = entityManager;
    }
}
