using Content.Shared.Construction;
using Content.Shared.Damage;

namespace Content.Server.Construction.Completions;

/// <summary>
/// Damage the entity on step completion.
/// </summary>
[DataDefinition]
public sealed partial class DamageEntity : IGraphAction
{
    /// <summary>
    /// Damage to deal to the entity.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage;

    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        entityManager.System<DamageableSystem>().TryChangeDamage(uid, Damage, origin: userUid);
    }
}
