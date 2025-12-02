using System.Linq;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;

namespace Content.Shared.Mobs.Systems;

public sealed partial class MobThresholdSystem
{
    /// <summary>
    /// Calculates the total damage from vital body parts (Head, Chest, Groin), for complex-bodies.
    /// For non-complex bodies or if no vital parts are found, returns the total damage from the target entity.
    /// </summary>
    /// <param name="target">The entity to check for vital damage</param>
    /// <param name="damageableComponent">The damageable component of the target entity</param>
    /// <returns>Total damage from vital body parts, or total damage if not a complex body or no vital parts found</returns>
    public FixedPoint2 CheckVitalDamage(EntityUid target, DamageableComponent damageableComponent)
    {
        var damage = damageableComponent.TotalDamage;

        if (!TryComp(target, out BodyComponent? body) ||
            body.BodyType != BodyType.Complex)
            return damage;

        if (body.RootContainer?.ContainedEntity is not { } rootPart)
            return damage;

        FixedPoint2 result = FixedPoint2.Zero;

        var criticalParts = new[]
        {
            BodyPartType.Head,
            BodyPartType.Chest,
            BodyPartType.Groin
        };

        foreach (var (woundable, _) in _wound.GetAllWoundableChildren(rootPart))
        {
            if (!TryComp(woundable, out DamageableComponent? wdc) ||
                !TryComp(woundable, out BodyPartComponent? bpc))
                continue;

            if (criticalParts.Contains(bpc.PartType))
                result += wdc.TotalDamage;
        }

        return result;
    }

}
