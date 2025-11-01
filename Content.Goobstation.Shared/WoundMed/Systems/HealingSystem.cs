using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Database;
using Content.Shared.Medical;
using Content.Shared.Medical._Goobstation;
using Content.Shared.Medical.Healing;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.WoundMed.Systems;

public sealed class WoundMedicationSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStackSystem _stacks = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HealingSystemEvent>(OnHealingSystemEvent);
    }

    /// <summary>
    ///     Returns the damage group for a given damage type.
    /// </summary>
    private string? GetDamageGroupByType(string id)
    {
        return (from @group in _prototypes.EnumeratePrototypes<DamageGroupPrototype>()
            where @group.DamageTypes.Contains(id)
            select @group.ID).FirstOrDefault();
    }

    /// <summary>
    /// Method <c>IsBodyDamaged</c> returns if a body part can be healed by the healing component. Returns false part is fully healed too.
    /// </summary>
    /// <param name="target">the target Entity</param>
    /// <param name="user">The person trying to heal. (optional)</param>
    /// <param name="healing">The healing component.</param>
    /// <param name="targetedPart">bypasses targeting system to specify a limb. Must be set if user is null. (optional)</param>
    /// <returns> Wether or not the targeted part can be healed. </returns>
    public bool IsBodyDamaged(Entity<BodyComponent> target, EntityUid? user, HealingComponent healing, EntityUid? targetedPart = null) // Goob edit: private => public, used in RepairableSystems.cs
    {
        if (user == null && targetedPart == null) // no limb can be targeted at all
            return false;

        if (user != null)
        {
            if (!TryComp<TargetingComponent>(user, out var targeting))
                return false;

            var (partType, symmetry) = _bodySystem.ConvertTargetBodyPart(targeting.Target);
            var targetedBodyPart = _bodySystem.GetBodyChildrenOfType(target, partType, target, symmetry).ToList().FirstOrNull();

            if (targetedBodyPart != null && targetedPart == null)
                targetedPart = targetedBodyPart.Value.Id;
        }

        if (targetedPart == null
            || !TryComp(targetedPart, out DamageableComponent? damageable))
        {
            if (user != null)
                _popupSystem.PopupClient(Loc.GetString("missing-body-part"), target, user.Value, PopupType.MediumCaution);
            return false;
        }

        if (healing.Damage.DamageDict.Keys
            .Any(damageKey => _wounds.GetWoundableSeverityPoint(
                targetedPart.Value,
                damageGroup: GetDamageGroupByType(damageKey),
                healable: true) > 0 || damageable.Damage.DamageDict[damageKey].Value > 0))
            return true;

        if (healing.BloodlossModifier == 0)
            return false;

        foreach (var wound in _wounds.GetWoundableWounds(targetedPart.Value))
        {
            if (!TryComp<BleedInflicterComponent>(wound, out var bleeds)
                || !bleeds.IsBleeding)
                continue;

            return true;
        }

        return false;
    }

    /// <summary>
    ///  Tries to return the first limb that has one of the damage type we are trying to heal
    /// </summary>
    /// <returns>  True or False if next damaged part exists. </returns>
    public bool TryGetNextDamagedPart(EntityUid ent, HealingComponent healing, out EntityUid? part) // used in RepairableSystems.cs
    {
        part = null;

        if (!TryComp<BodyComponent>(ent, out var body))
            return false;

        var parts = _bodySystem.GetBodyChildren(ent, body).ToArray();
        foreach (var limb in parts)
        {
            part = limb.Id;
            if (IsBodyDamaged((ent, body), null, healing, limb.Id))
                return true;
        }
        return false;
    }

    private void OnBodyDoAfter(EntityUid ent, BodyComponent comp, ref HealingDoAfterEvent args)
    {
        var dontRepeat = false;

        if (!TryComp(args.Used, out HealingComponent? healing))
            return;

        if (args.Handled || args.Cancelled)
            return;

        var targetedWoundable = EntityUid.Invalid;
        if (TryComp<TargetingComponent>(args.User, out var targeting))
        {
            var (partType, symmetry) = _bodySystem.ConvertTargetBodyPart(targeting.Target);
            var targetedBodyPart = _bodySystem.GetBodyChildrenOfType(ent, partType, comp, symmetry).ToList().FirstOrDefault();
            targetedWoundable = targetedBodyPart.Id;
        }

        // Check if there is anything to heal on the initial limb target
        if (!IsBodyDamaged((ent, comp), null, healing, targetedWoundable))
        {
            // If not then get the next limb to heal
            if (TryGetNextDamagedPart(ent, healing, out var limbTemp) && limbTemp != null)
                targetedWoundable = limbTemp.Value;
        }

        if (targetedWoundable == EntityUid.Invalid)
            _popupSystem.PopupClient(
                Loc.GetString("medical-item-cant-use", ("item", args.Used)),
                ent,
                args.User,
                PopupType.MediumCaution);

        if (!TryComp<WoundableComponent>(targetedWoundable, out var woundableComp)
            || !TryComp<DamageableComponent>(targetedWoundable, out var damageableComp))
            return;

        var healedBleed = false;
        var canHeal = true;
        var healedTotal = FixedPoint2.Zero;
        FixedPoint2 modifiedBleedStopAbility = 0;
        // Heal some bleeds
        bool healedBleedWound = false;
        bool healedBleedLevel = false;
        if (healing.BloodlossModifier != 0)
        {
            healedBleedWound = _wounds.TryHealBleedingWounds(targetedWoundable, healing.BloodlossModifier, out modifiedBleedStopAbility, woundableComp);
            if (healedBleedWound)
                _popupSystem.PopupClient(modifiedBleedStopAbility > 0
                        ? Loc.GetString("rebell-medical-item-stop-bleeding-fully")
                        : Loc.GetString("rebell-medical-item-stop-bleeding-partially"),
                    ent,
                    args.User);
        }

        if (healing.ModifyBloodLevel != 0)
            healedBleedLevel = _bloodstreamSystem.TryModifyBloodLevel(ent, -healing.ModifyBloodLevel);

        healedBleed = healedBleedWound || healedBleedLevel;

        if (TraumaSystem.TraumasBlockingHealing.Any(traumaType => _trauma.HasWoundableTrauma(targetedWoundable, traumaType, woundableComp, false)))
        {
            canHeal = false;

            if (!healedBleed)
                _popupSystem.PopupClient(Loc.GetString("medical-item-requires-surgery-rebell", ("target", ent)), ent, args.User, PopupType.MediumCaution);
            return;
        }

        if (canHeal)
        {
            if (healing.BloodlossModifier == 0 && healing.ModifyBloodLevel >= 0 && woundableComp.Bleeds > 0)  // If the healing item has no bleeding heals, and its bleeding, we raise the alert. Goobstation edit
                _popupSystem.PopupClient(Loc.GetString("medical-item-cant-use-rebell", ("target", ent)), ent, args.User);

            var damageChanged = _damageable.TryChangeDamage(targetedWoundable, healing.Damage * _damageable.UniversalTopicalsHealModifier, true, origin: args.User, ignoreBlockers: healedBleed || healing.BloodlossModifier == 0); // GOOBEDIT

            if (damageChanged is not null)
                healedTotal += -damageChanged.GetTotal();
        }

        // Re-verify that we can heal the damage.
        if (TryComp<StackComponent>(args.Used.Value, out var stackComp))
        {
            _stacks.Use(args.Used.Value, 1, stackComp);

            if (_stacks.GetCount(args.Used.Value, stackComp) <= 0)
                dontRepeat = true;
        }
        else
            QueueDel(args.Used.Value);

        if (ent != args.User)
            _adminLogger.Add(LogType.Healed,
                $"{EntityManager.ToPrettyString(args.User):user} healed {EntityManager.ToPrettyString(ent):target} for {healedTotal:damage} damage");
        else
            _adminLogger.Add(LogType.Healed,
                $"{EntityManager.ToPrettyString(args.User):user} healed themselves for {healedTotal:damage} damage");

        _audio.PlayPredicted(healing.HealingEndSound, ent, ent, AudioParams.Default.WithVariation(0.125f).WithVolume(1f)); // Goob edit

        // Logic to determine whether or not to repeat the healing action
        args.Repeat = IsAnythingToHeal(args.User, ent, (args.Used.Value, healing));
        args.Handled = true;

        if (args.Repeat || dontRepeat)
            return;

        if (modifiedBleedStopAbility != -healing.BloodlossModifier)
            _popupSystem.PopupClient(Loc.GetString("medical-item-finished-using", ("item", args.Used)), ent, args.User, PopupType.Medium);
    }

    /// <summary>
    /// Method <c>IsAnythingToHeal</c> returns if a target can be healed by the healing component.
    /// </summary>
    /// <returns>Returns false if the target is fully healed.</returns>
    private bool IsAnythingToHeal(EntityUid user, EntityUid target, Entity<HealingComponent> healing)
    {
        // Check if the target has damageable component
        if (!TryComp<DamageableComponent>(target, out var targetDamage))
            return false;

        // Check damage based on container restrictions
        var hasDamage = healing.Comp.DamageContainers is null ||
                        targetDamage.DamageContainerID is null ||
                        healing.Comp.DamageContainers.Contains(targetDamage.DamageContainerID.Value);

        // Check body damage if applicable
        var hasBodyDamage = TryComp<BodyComponent>(target, out var bodyComp) &&
                            IsBodyDamaged((target, bodyComp), user, healing.Comp);

        // Check blood loss if applicable
        var hasBloodLoss = healing.Comp.ModifyBloodLevel > 0 &&
                           TryComp<BloodstreamComponent>(target, out var bloodstream) &&
                           _solutionContainerSystem.ResolveSolution(target,
                               bloodstream.BloodSolutionName,
                               ref bloodstream.BloodSolution,
                               out var bloodSolution) &&
                           bloodSolution.Volume < bloodSolution.MaxVolume;

        return hasDamage || hasBodyDamage || hasBloodLoss;
    }

    #region Events
    /// <summary>
    /// Called in Content.Shared inside TryHeal to check if there's anything to heal
    /// </summary>
    /// <param name="args">Entity<DamageableComponent?> target</param>
    private void OnHealingSystemEvent(ref HealingSystemEvent args)
    {
        if (!TryComp<DamageableComponent>(args.Entity, out var damageable))
        {
            args.AnythingToDo = false;
            return;
        }

        // Get the healing component from the event's target entity
        if (!TryComp<HealingComponent>(args.Entity, out var healing))
        {
            args.AnythingToDo = false;
            return;
        }

        // Check if there's any damage to heal
        var hasDamage = damageable.Damage.GetTotal() > FixedPoint2.Zero;
        // Check body damage if applicable
        var hasBodyDamage = TryComp<BodyComponent>(args.Entity, out var bodyComp) &&
                            IsBodyDamaged((args.Entity, bodyComp), null, healing);

        // Check blood loss if applicable
        var hasBloodLoss = healing.ModifyBloodLevel < 0 &&
                           TryComp<BloodstreamComponent>(args.Entity, out var bloodstream) &&
                           _solutionContainerSystem.ResolveSolution(args.Entity,
                               bloodstream.BloodSolutionName,
                               ref bloodstream.BloodSolution,
                               out var bloodSolution) &&
                           bloodSolution.Volume < bloodSolution.MaxVolume;

        args.AnythingToDo = hasDamage || hasBodyDamage || hasBloodLoss;
    }
    #endregion
}
