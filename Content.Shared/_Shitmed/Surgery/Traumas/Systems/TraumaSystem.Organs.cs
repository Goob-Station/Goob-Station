using System.Linq;
using Content.Shared._Shitmed.Surgery.Wounds.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.FixedPoint;
using Robust.Shared.Random;

namespace Content.Shared._Shitmed.Surgery.Traumas.Systems;

public partial class TraumaSystem
{
    private const float OrganDamageSeverityThreshold = 12f;

    private void InitOrgans()
    {
        SubscribeLocalEvent<OrganComponent, OrganDamageSeverityChanged>(SomeShitHappensHere);
        SubscribeLocalEvent<OrganComponent, OrganDamagePointChangedEvent>(SomeOtherShitHappensHere);
    }

    #region Event handling

    private void SomeShitHappensHere(EntityUid organ, OrganComponent comp, OrganDamageSeverityChanged args)
    {

        // TODO: Make those actually handled. and also make undamaged organs fall out when a woundable gets destroyed, for the funnies :3
    }

    private void SomeOtherShitHappensHere(EntityUid organ, OrganComponent comp, OrganDamagePointChangedEvent args)
    {

    }

    #endregion

    #region Public API

    public bool ApplyDamageToOrgan(EntityUid organ, FixedPoint2 severity, OrganComponent? organComp = null)
    {
        if (!Resolve(organ, ref organComp) || _net.IsClient)
            return false;

        var newIntegrity = FixedPoint2.Clamp(organComp.OrganIntegrity - severity, 0, organComp.IntegrityCap);
        if (organComp.OrganIntegrity == newIntegrity)
            return false;

        organComp.OrganIntegrity = newIntegrity;
        var ev = new OrganDamagePointChangedEvent(organ, organComp.OrganIntegrity, severity);
        RaiseLocalEvent(organ, ref ev, true);

        UpdateOrganIntegrity(organ, organComp);
        Dirty(organ, organComp);
        return true;
    }

    public bool TryCreateOrganDamageModifier(EntityUid uid,
        FixedPoint2 severity,
        EntityUid effectOwner,
        string identifier,
        OrganComponent? organ = null)
    {
        if (!Resolve(uid, ref organ))
            return false;

        organ.IntegrityModifiers.Add((identifier, effectOwner), severity);
        UpdateOrganIntegrity(uid, organ);

        return true;
    }

    public bool TrySetOrganDamageModifier(EntityUid uid,
        FixedPoint2 severity,
        EntityUid effectOwner,
        string identifier,
        OrganComponent? organ = null)
    {
        if (!Resolve(uid, ref organ))
            return false;

        organ.IntegrityModifiers[(identifier, effectOwner)] = severity;
        UpdateOrganIntegrity(uid, organ);

        return true;
    }

    public bool TryChangeOrganDamageModifier(EntityUid uid,
        FixedPoint2 change,
        EntityUid effectOwner,
        string identifier,
        OrganComponent? organ = null)
    {
        if (!Resolve(uid, ref organ))
            return false;

        if (!organ.IntegrityModifiers.TryGetValue((identifier, effectOwner), out var value))
            return false;

        organ.IntegrityModifiers[(identifier, effectOwner)] = value + change;
        UpdateOrganIntegrity(uid, organ);

        return true;
    }

    public bool TryRemoveOrganDamageModifier(EntityUid uid,
        EntityUid effectOwner,
        string identifier,
        OrganComponent? organ = null)
    {
        if (!Resolve(uid, ref organ))
            return false;

        if (!organ.IntegrityModifiers.Remove((identifier, effectOwner)))
            return false;

        UpdateOrganIntegrity(uid, organ);
        return true;
    }

    public bool RandomOrganTraumaChance(EntityUid target, EntityUid woundInflicter, WoundableComponent woundable, FixedPoint2 severity)
    {
        var bodyPart = Comp<BodyPartComponent>(target);
        if (!bodyPart.Body.HasValue)
            return false; // No entity to apply pain to

        if (severity < OrganDamageSeverityThreshold)
            return false;

        var totalIntegrity =
            _body.GetPartOrgans(target, bodyPart)
                .Aggregate((FixedPoint2)0, (current, organ) => current + organ.Component.OrganIntegrity);

        if (totalIntegrity <= 0) // No surviving organs
            return false;

        // organ damage is like, very deadly, but not yet
        // so like, like, yeah, we don't want a disabler to induce some EVIL ASS organ damage with a 0,000001% chance and ruin your round
        // Very unlikely to happen if your woundables are in a good condition
        var chance =
            FixedPoint2.Clamp(
                woundable.WoundableIntegrity / woundable.IntegrityCap / totalIntegrity
                + Comp<WoundComponent>(woundInflicter).TraumasChances[TraumaType.OrganDamage],
                0,
                1);

        return _random.Prob((float) chance);
    }

    #endregion

    #region Private API

    private void UpdateOrganIntegrity(EntityUid uid, OrganComponent organ)
    {
        var oldIntegrity = organ.OrganIntegrity;

        // Makes more sense for this to just multiply the value no?
        if (organ.IntegrityModifiers.Count > 0)
            organ.OrganIntegrity *= FixedPoint2.Clamp(organ.IntegrityModifiers
                .Aggregate((FixedPoint2) 0,
                    (current, modifier) => current + modifier.Value),
                0,
                organ.IntegrityCap);

        if (oldIntegrity != organ.OrganIntegrity)
        {
            var ev = new OrganDamagePointChangedEvent(uid, organ.OrganIntegrity, organ.OrganIntegrity - oldIntegrity);
            RaiseLocalEvent(uid, ref ev, true);

            // TODO: might want also to raise the event on the woundable.
        }

        var nearestSeverity = organ.OrganSeverity;
        foreach (var (severity, value) in organ.IntegrityThresholds.OrderByDescending(kv => kv.Value))
        {
            if (organ.OrganIntegrity < value)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity != organ.OrganSeverity)
        {
            var ev = new OrganDamageSeverityChanged(uid, organ.OrganSeverity, nearestSeverity);
            RaiseLocalEvent(uid, ref ev, true);
            // TODO: might want also to raise the event on the woundable.
        }
        organ.OrganSeverity = nearestSeverity;
        Dirty(uid, organ);
    }

    #endregion
}
