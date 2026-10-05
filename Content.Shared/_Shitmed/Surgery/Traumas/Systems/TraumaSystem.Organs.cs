using System.Linq;
using Content.Shared._Shitmed.CCVar;
using Content.Shared._Shitmed.Medical.Surgery.Pain;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared.Body.Organ;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;

public partial class TraumaSystem
{
    private const string OrganDamagePainIdentifier = "OrganDamage";

    private static readonly FixedPoint2 OrganDamagePainBudget = 30;
    public static readonly EntProtoId OrgansDamagedSlowdown = "OrgansDamagedSlowdownEffect";

    private void InitOrgans()
    {
        SubscribeLocalEvent<WoundableComponent, OrganIntegrityChangedEventOnWoundable>(OnOrganIntegrityOnWoundableChanged);
        SubscribeLocalEvent<OrganComponent, OrganIntegrityChangedEvent>(OnOrganIntegrityChanged);
        SubscribeLocalEvent<WoundableComponent, OrganDamageSeverityChangedOnWoundable>(OnOrganSeverityChanged);
    }

    #region Event handling

    private void OnOrganIntegrityOnWoundableChanged(Entity<WoundableComponent> bodyPart, ref OrganIntegrityChangedEventOnWoundable args)
    {
        if (args.Organ.Comp.Body == null)
            return;

        if (!_consciousness.TryGetNerveSystem(args.Organ.Comp.Body.Value, out var nerveSys))
            return;

        var organs = _body.GetPartOrgans(args.Organ.Comp.Body.Value).ToList();
        var totalIntegrity = organs.Aggregate(FixedPoint2.Zero, (current, organ) => current + organ.Component.OrganIntegrity);
        var totalIntegrityCap = organs.Aggregate(FixedPoint2.Zero, (current, organ) => current + organ.Component.IntegrityCap);

        var damageFraction = totalIntegrityCap > 0
            ? (totalIntegrityCap - totalIntegrity) / totalIntegrityCap
            : FixedPoint2.Zero;
        var organPain = damageFraction * OrganDamagePainBudget;

        if (!_pain.TryChangePainModifier(
                nerveSys.Value,
                bodyPart.Owner,
                OrganDamagePainIdentifier,
                organPain,
                nerveSys.Value.Comp))
        {
            _pain.TryAddPainModifier(
                nerveSys.Value,
                bodyPart.Owner,
                OrganDamagePainIdentifier,
                organPain,
                PainDamageTypes.TraumaticPain,
                nerveSys.Value.Comp);
        }
    }

    private void OnOrganIntegrityChanged(Entity<OrganComponent> organ, ref OrganIntegrityChangedEvent args)
    {
        if (organ.Comp.Body == null)
            return;

        if (args.NewIntegrity < organ.Comp.IntegrityCap || !TryGetBodyTraumas(organ.Comp.Body.Value, out var traumas, OrganDamage))
            return;

        foreach (var trauma in traumas.Where(trauma => trauma.Comp.TraumaTarget == organ))
        {
            RemoveTrauma(trauma);
        }
    }

    private void OnOrganSeverityChanged(Entity<WoundableComponent> bodyPart, ref OrganDamageSeverityChangedOnWoundable args)
    {
        var body = args.Organ.Comp.Body;
        if (body == null
            || args.NewSeverity < args.OldSeverity)
            return;

        _popup.PopupClient(Loc.GetString($"popup-trauma-OrganDamage-{args.NewSeverity.ToString()}", ("part", bodyPart)),
            body.Value,
            body.Value,
            PopupType.SmallCaution);

        if (args.NewSeverity != OrganSeverity.Destroyed)
            return;

        if (_consciousness.TryGetNerveSystem(body.Value, out var nerveSys)
            && !_mobState.IsDead(body.Value))
        {
            var sex = Sex.Unsexed;
            if (TryComp<HumanoidAppearanceComponent>(body, out var humanoid))
                sex = humanoid.Sex;

            if (nerveSys.Value.Comp.OrganDestructionReflexSounds.TryGetValue(sex, out var reflexSound))
                _pain.PlayPainSoundWithCleanup(
                    body.Value,
                    nerveSys.Value.Comp,
                    reflexSound,
                    AudioParams.Default.WithVolume(6f));

            _stun.TryUpdateParalyzeDuration(body.Value, nerveSys.Value.Comp.OrganDamageStunTime);
            _movementMod.TryUpdateMovementSpeedModDuration(
                 body.Value,
                 OrgansDamagedSlowdown,
                 nerveSys.Value.Comp.OrganDamageStunTime * _cfg.GetCVar(SurgeryCVars.OrganTraumaSlowdownTimeMultiplier),
                 _cfg.GetCVar(SurgeryCVars.OrganTraumaWalkSpeedSlowdown),
                 _cfg.GetCVar(SurgeryCVars.OrganTraumaRunSpeedSlowdown));
        }

        if (TryGetWoundableTrauma(bodyPart, out var traumas, OrganDamage, bodyPart))
        {
            foreach (var trauma in traumas)
            {
                if (trauma.Comp.TraumaTarget != args.Organ)
                    continue;

                RemoveTrauma(trauma);
            }
        }

        _audio.PlayPvs(args.Organ.Comp.OrganDestroyedSound, body.Value);

        if (TryComp<OrganDeathTraumaComponent>(args.Organ, out var deathTrauma)
            && !HasOrganTrauma(bodyPart, args.Organ, deathTrauma.Trauma))
            TryInflictOrganTrauma(bodyPart, args.Organ, deathTrauma.Trauma, args.Organ.Comp.IntegrityCap);

        if (args.Organ.Comp.Indestructible)
            return;

        _body.RemoveOrgan(args.Organ, args.Organ.Comp);

        if (_net.IsServer)
            QueueDel(args.Organ);
    }

    private bool HasOrganTrauma(EntityUid woundable, EntityUid organ, ProtoId<TraumaTypePrototype> traumaType)
    {
        if (!TryGetWoundableTrauma(woundable, out var traumas, traumaType))
            return false;

        foreach (var trauma in traumas)
        {
            if (trauma.Comp.TraumaTarget == organ)
                return true;
        }

        return false;
    }

    public void RestoreOrganIntegrity(EntityUid uid, OrganComponent? organ = null)
    {
        if (!Resolve(uid, ref organ))
            return;

        organ.IntegrityModifiers.Clear();
        UpdateOrganIntegrity(uid, organ);
    }

    #endregion

    #region Public API
    public bool TryCreateOrganDamageModifier(EntityUid uid,
        FixedPoint2 severity,
        EntityUid effectOwner,
        string identifier,
        OrganComponent? organ = null)
    {
        if (severity == 0
            || !Resolve(uid, ref organ))
            return false;

        if (!organ.IntegrityModifiers.TryAdd((identifier, effectOwner), severity))
            return false;

        UpdateOrganIntegrity(uid, organ);

        return true;
    }

    public bool TrySetOrganDamageModifier(EntityUid uid,
        FixedPoint2 severity,
        EntityUid effectOwner,
        string identifier,
        OrganComponent? organ = null)
    {
        if (severity == 0
            || !Resolve(uid, ref organ))
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
        if (change == 0
            || !Resolve(uid, ref organ))
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

        if (TryComp<TraumaComponent>(effectOwner, out var traumaComp))
            RemoveTrauma((effectOwner, traumaComp));

        UpdateOrganIntegrity(uid, organ);
        return true;
    }

    #endregion

    #region Private API

    private void UpdateOrganIntegrity(EntityUid uid, OrganComponent organ)
    {
        var oldIntegrity = organ.OrganIntegrity;

        var totalDamage = FixedPoint2.Zero;
        foreach (var modifier in organ.IntegrityModifiers)
            totalDamage += modifier.Value;

        organ.OrganIntegrity = FixedPoint2.Clamp(organ.IntegrityCap - totalDamage, 0, organ.IntegrityCap);

        _container.TryGetContainingContainer((uid, Transform(uid), MetaData(uid)), out var container);

        if (oldIntegrity != organ.OrganIntegrity)
        {
            var ev = new OrganIntegrityChangedEvent(oldIntegrity, organ.OrganIntegrity);
            RaiseLocalEvent(uid, ref ev);

            if (container != null)
            {
                var ev1 = new OrganIntegrityChangedEventOnWoundable((uid, organ), oldIntegrity, organ.OrganIntegrity);
                RaiseLocalEvent(container.Owner, ref ev1);
            }
        }

        organ.SortedIntegrityThresholds ??= organ.IntegrityThresholds.OrderBy(kv => kv.Value).ToArray();

        var nearestSeverity = organ.SortedIntegrityThresholds.Length > 0
            ? organ.SortedIntegrityThresholds[^1].Key
            : organ.OrganSeverity;
        foreach (var (severity, value) in organ.SortedIntegrityThresholds)
        {
            if (organ.OrganIntegrity > value)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity != organ.OrganSeverity)
        {
            var ev = new OrganDamageSeverityChanged(organ.OrganSeverity, nearestSeverity);
            RaiseLocalEvent(uid, ref ev);
            if (container != null)
            {
                var ev1 = new OrganDamageSeverityChangedOnWoundable((uid, organ), organ.OrganSeverity, nearestSeverity);
                RaiseLocalEvent(container.Owner, ref ev1);
            }
        }

        organ.OrganSeverity = nearestSeverity;
        Dirty(uid, organ);
    }

    #endregion
}
