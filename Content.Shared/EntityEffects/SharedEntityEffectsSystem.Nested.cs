using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects;

/// <summary>
/// Entity effects API counterparts using <see cref="EntityEffectPrototype"/> instead of <see cref="EntityEffect"/>.
/// </summary>
public sealed partial class SharedEntityEffectsSystem
{
    /// <summary>
    /// <c>TryApplyEffect</c> overload using a <see cref="EntityEffectPrototype"/> instead of <see cref="EntityEffect"/>.
    /// </summary>
    public void TryApplyEffect(EntityUid target, [ForbidLiteral] ProtoId<EntityEffectPrototype> id, float scale = 1f, EntityUid? user = null,
        bool predicted = true) // Trauma - Added predicted
    {
        var proto = _proto.Index(id); // Goobstation - Change _proto to ProtoMan when engine update
        if (_condition.TryConditions(target, proto.Conditions))
            ApplyEffects(target, proto.Effects, scale, user, predicted: predicted); // Trauma
    }
}
