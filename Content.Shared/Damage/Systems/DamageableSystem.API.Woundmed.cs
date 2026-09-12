using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Damage.Systems;

public sealed partial class DamageableSystem
{
    /// <summary>
    ///     [Woundmed]
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="origin"></param>
    /// <param name="targetPart"></param>
    /// <param name="damageSpecifier"></param>
    /// <param name="ignoreResistances"></param>
    /// <param name="partMultiplier"></param>
    /// <returns></returns>
    public Dictionary<string, FixedPoint2> DamageSpecifierToWoundList(
        Entity<DamageableComponent> ent,
        EntityUid? origin,
        TargetBodyPart targetPart,
        DamageSpecifier damageSpecifier,
        bool ignoreResistances = false,
        float partMultiplier = 1.00f)
    {
        var damageDict = new Dictionary<string, FixedPoint2>();

        damageSpecifier = ApplyUniversalAllModifiers(damageSpecifier);

        // some wounds like Asphyxiation and Bloodloss aren't supposed to be created.
        if (!ignoreResistances)
        {
            if (ent.Comp.DamageModifierSetId != null &&
                _prototypeManager.TryIndex(ent.Comp.DamageModifierSetId, out var modifierSet))
            {
                // lol bozo
                var spec = new DamageSpecifier
                {
                    DamageDict = damageSpecifier.DamageDict,
                };

                damageSpecifier = DamageSpecifier.ApplyModifierSet(spec, modifierSet);
            }

            var ev = new DamageModifyEvent(ent, damageSpecifier, origin, targetPart);
            RaiseLocalEvent(ent, ev);
            damageSpecifier = ev.Damage;

            if (damageSpecifier.Empty)
            {
                return damageDict;
            }
        }

        foreach (var (type, severity) in damageSpecifier.DamageDict)
        {
            if (!_prototypeManager.TryIndex<EntityPrototype>(type, out var woundPrototype)
                || !woundPrototype.TryGetComponent<WoundComponent>(out _, _factory)
                || severity <= 0)
                continue;

            damageDict.Add(type, severity * partMultiplier);
        }

        return damageDict;
    }

    /// <summary>
    ///     [Woundmed]
    ///     Change the DamageContainer of a DamageableComponent. - Goobstation, Rubin Code
    /// </summary>
    public void ChangeDamageContainer(EntityUid uid, string newDamageContainerId, DamageableComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false)
            || newDamageContainerId == component.DamageContainerID)
        {
            return;
        }

        // Try to get the new DamageContainerPrototype
        if (!_prototypeManager.TryIndex<DamageContainerPrototype>(newDamageContainerId, out var damageContainerPrototype))
        {
            // Return early if no DamageContainerPrototype is found
            return;
        }

        // Update the DamageContainerID
        component.DamageContainerID = new ProtoId<DamageContainerPrototype>(newDamageContainerId);

        // Clear the existing damage dictionary
        component.Damage.DamageDict.Clear();

        // Initialize damage dictionary, using the types and groups from the damage container prototype
        foreach (var type in damageContainerPrototype.SupportedTypes)
        {
            component.Damage.DamageDict.TryAdd(type, FixedPoint2.Zero);
        }

        foreach (var groupId in damageContainerPrototype.SupportedGroups)
        {
            var group = _prototypeManager.Index<DamageGroupPrototype>(groupId);
            foreach (var type in group.DamageTypes)
            {
                component.Damage.DamageDict.TryAdd(type, FixedPoint2.Zero);
            }
        }

        component.Damage.GetDamagePerGroup(_prototypeManager, component.DamagePerGroup);
        component.TotalDamage = component.Damage.GetTotal();
    }

    /// <summary>
    /// [Woundmed] [DeltaV]
    /// This is used to change DamageContainer to make cultists vulnerable to Holy Damage.
    /// </summary>
    public void SetDamageContainerID(Entity<DamageableComponent?> ent, string damageContainerId)
    {
        if (!_damageableQuery.Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.DamageContainerID = damageContainerId;
        Dirty(ent);
    }
}