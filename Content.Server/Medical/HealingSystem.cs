using Content.Server.Administration.Logs;
using Content.Server.Body.Systems;
using Content.Server.Medical.Components;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;

// Shitmed Change
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Surgery.Wounds.Systems;
using Robust.Shared.Audio;
using System.Linq;

namespace Content.Server.Medical;

public sealed class HealingSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTargetingSystem _targetingSystem = default!; // Shitmed Change
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly StackSystem _stacks = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!; // Shitmed Change
    [Dependency] private readonly WoundSystem _wounds = default!; // Shitmed Change

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HealingComponent, UseInHandEvent>(OnHealingUse);
        SubscribeLocalEvent<HealingComponent, AfterInteractEvent>(OnHealingAfterInteract);
        SubscribeLocalEvent<DamageableComponent, HealingDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<BodyComponent, HealingDoAfterEvent>(OnBodyDoAfter);

    }

    private void OnDoAfter(Entity<DamageableComponent> entity, ref HealingDoAfterEvent args)
    {
        var dontRepeat = false;

        // Shitmed Change: Consciousness check because some body entities don't have Consciousness
        if (!TryComp(args.Used, out HealingComponent? healing) || HasComp<BodyComponent>(entity) && HasComp<ConsciousnessComponent>(entity))
            return;

        if (args.Handled || args.Cancelled)
            return;

        if (healing.DamageContainers is not null &&
            entity.Comp.DamageContainerID is not null &&
            !healing.DamageContainers.Contains(entity.Comp.DamageContainerID))
        {
            return;
        }

        // Heal some bloodloss damage.
        if (healing.BloodlossModifier != 0)
        {
            if (!TryComp<BloodstreamComponent>(entity, out var bloodstream))
                return;
            var isBleeding = bloodstream.BleedAmount > 0;
            _bloodstreamSystem.TryModifyBleedAmount(entity.Owner, healing.BloodlossModifier);
            if (isBleeding != bloodstream.BleedAmount > 0)
            {
                var popup = (args.User == entity.Owner)
                    ? Loc.GetString("medical-item-stop-bleeding-self")
                    : Loc.GetString("medical-item-stop-bleeding", ("target", Identity.Entity(entity.Owner, EntityManager)));
                _popupSystem.PopupEntity(popup, entity, args.User);
            }
        }

        // Restores missing blood
        if (healing.ModifyBloodLevel != 0)
            _bloodstreamSystem.TryModifyBloodLevel(entity.Owner, healing.ModifyBloodLevel);

        var healed = _damageable.TryChangeDamage(entity.Owner, healing.Damage * _damageable.UniversalTopicalsHealModifier, true, origin: args.Args.User); // Shitmed Change

        if (healed == null && healing.BloodlossModifier != 0)
            return;

        var total = healed?.GetTotal() ?? FixedPoint2.Zero;

        // Re-verify that we can heal the damage.

        if (TryComp<StackComponent>(args.Used.Value, out var stackComp))
        {
            _stacks.Use(args.Used.Value, 1, stackComp);

            if (_stacks.GetCount(args.Used.Value, stackComp) <= 0)
                dontRepeat = true;
        }
        else
        {
            QueueDel(args.Used.Value);
        }

        if (entity.Owner != args.User)
        {
            _adminLogger.Add(LogType.Healed,
                $"{EntityManager.ToPrettyString(args.User):user} healed {EntityManager.ToPrettyString(entity.Owner):target} for {total:damage} damage");
        }
        else
        {
            _adminLogger.Add(LogType.Healed,
                $"{EntityManager.ToPrettyString(args.User):user} healed themselves for {total:damage} damage");
        }

        _audio.PlayPvs(healing.HealingEndSound, entity.Owner, AudioParams.Default.WithVariation(0.125f).WithVolume(1f));

        // Logic to determine the whether or not to repeat the healing action
        args.Repeat = HasDamage(entity, healing) && !dontRepeat; // Shitmed Change
        if (!args.Repeat && !dontRepeat)
            _popupSystem.PopupEntity(Loc.GetString("medical-item-finished-using", ("item", args.Used)), entity.Owner, args.User);
        args.Handled = true;
    }

    private bool HasDamage(Entity<DamageableComponent> ent, HealingComponent healing)
    {
        var damageableDict = ent.Comp.Damage.DamageDict;
        var healingDict = healing.Damage.DamageDict;
        foreach (var type in healingDict)
        {
            if (damageableDict[type.Key].Value > 0)
            {
                return true;
            }
        }

        if (TryComp<BloodstreamComponent>(ent, out var bloodstream))
        {
            // Is ent missing blood that we can restore?
            if (healing.ModifyBloodLevel > 0
                && _solutionContainerSystem.ResolveSolution(ent.Owner, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution)
                && bloodSolution.Volume < bloodSolution.MaxVolume)
            {
                return true;
            }

            // Is ent bleeding and can we stop it?
            if (healing.BloodlossModifier < 0 && bloodstream.BleedAmount > 0)
            {
                return true;
            }
        }

        return false;
    }

    // Shitmed Change Start
    private bool IsBodyDamaged(Entity<BodyComponent> target, EntityUid user, HealingComponent healing)
    {
        if (!TryComp<TargetingComponent>(user, out var targeting))
            return false;

        var (partType, symmetry) = _bodySystem.ConvertTargetBodyPart(targeting.Target);
        var targetedBodyPart = _bodySystem.GetBodyChildrenOfType(target, partType, target, symmetry).ToList().FirstOrDefault();

        if (healing.Damage.DamageDict.Keys.ToList()
            .Any(damage => _wounds.HasDamageOfGroup(targetedBodyPart.Id, damage) || _wounds.HasDamageOfType(targetedBodyPart.Id, damage)))
            return true;

        if (healing.BloodlossModifier != 0)
        {
            foreach (var wound in _wounds.GetWoundableWounds(targetedBodyPart.Id))
            {
                if (!TryComp<BleedInflicterComponent>(wound, out var bleeds) || !bleeds.IsBleeding)
                    continue;

                return true;
            }
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

        var stuffToHeal = new Dictionary<string, FixedPoint2>();
        var targetedWoundable = EntityUid.Invalid;
        if (TryComp<TargetingComponent>(args.User, out var targeting))
        {
            var (partType, symmetry) = _bodySystem.ConvertTargetBodyPart(targeting.Target);
            var targetedBodyPart = _bodySystem.GetBodyChildrenOfType(ent, partType, comp, symmetry).ToList().FirstOrDefault();

            foreach (var damage in
                     healing.Damage.DamageDict.Where(damage =>
                         _wounds.HasDamageOfGroup(targetedBodyPart.Id, damage.Key)
                         || _wounds.HasDamageOfType(targetedBodyPart.Id, damage.Key)))
            {
                stuffToHeal.Add(damage.Key, damage.Value);
            }

            if (stuffToHeal.Count > 0)
                targetedWoundable = targetedBodyPart.Id;
        }

        if (targetedWoundable == EntityUid.Invalid)
        {
            _popupSystem.PopupEntity(Loc.GetString("does-not-exist-rebell"), ent, args.User, PopupType.MediumCaution);
            return;
        }

        var woundableDamageContainer = Comp<WoundableComponent>(targetedWoundable).DamageContainerID;
        if (healing.DamageContainers is not null &&
            woundableDamageContainer is not null &&
            !healing.DamageContainers.Contains(woundableDamageContainer))
        {
            _popupSystem.PopupEntity(
                Loc.GetString("cant-heal-damage-container-rebell", ("target", ent), ("used", args.Used)),
                ent,
                args.User,
                PopupType.MediumCaution);
            return;
        }

        // Heal some bleeds
        if (healing.BloodlossModifier != 0)
        {
            var bleedStopAbility = -(FixedPoint2) healing.BloodlossModifier;
            foreach (var wound in _wounds.GetWoundableWounds(targetedWoundable))
            {
                if (!TryComp<BleedInflicterComponent>(wound, out var bleeds) || !bleeds.IsBleeding)
                    continue;

                if (bleedStopAbility > bleeds.BleedingAmount)
                {
                    bleedStopAbility -= bleeds.BleedingAmountRaw;
                    bleeds.BleedingAmountRaw = 0;
                    bleeds.IsBleeding = false;

                    wound.Comp.CanBeHealed = true;
                }
                else
                {
                    bleeds.BleedingAmountRaw -= bleedStopAbility;
                }

                bleedStopAbility -= bleeds.BleedingAmount;
            }
            _bloodstreamSystem.TryModifyBleedAmount(ent, healing.ModifyBloodLevel);

            if (bleedStopAbility != -healing.BloodlossModifier)
            {
                _popupSystem.PopupEntity(bleedStopAbility > 0
                        ? Loc.GetString("rebell-medical-item-stop-bleeding-fully")
                        : Loc.GetString("rebell-medical-item-stop-bleeding-partially"),
                    ent,
                    args.User);
            }
        }

        if (targetedWoundable == EntityUid.Invalid
            || stuffToHeal.Count == 0
            || healing.BloodlossModifier <= 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("medical-item-cant-use-rebell", ("target", ent)), ent, args.User);
            return;
        }

        // Re-verify that we can heal the damage.
        if (TryComp<StackComponent>(args.Used.Value, out var stackComp))
        {
            _stacks.Use(args.Used.Value, 1, stackComp);

            if (_stacks.GetCount(args.Used.Value, stackComp) <= 0)
                dontRepeat = true;
        }
        else
        {
            QueueDel(args.Used.Value);
        }

        var healedTotal = (FixedPoint2) 0;
        foreach (var (key, value) in stuffToHeal)
        {
            if (_wounds.TryHealWoundsOnWoundable(targetedWoundable, -value, key, out var healed))
            {
                healedTotal += healed;
                continue;
            }

            // Check for a group if it's not a type
            if (_wounds.TryHealWoundsOnWoundable(targetedWoundable, -value, out var healedGroup, damageGroup: key))
            {
                healedTotal += healedGroup;
            }
        }

        if (healedTotal <= 0)
        {
            if (healing.BloodlossModifier == 0)
                _popupSystem.PopupEntity(Loc.GetString("medical-item-cant-use", ("item", args.Used)), ent, args.User);

            return;
        }

        if (ent != args.User)
        {
            _adminLogger.Add(LogType.Healed,
                $"{EntityManager.ToPrettyString(args.User):user} healed {EntityManager.ToPrettyString(ent):target} for {healedTotal:damage} damage");
        }
        else
        {
            _adminLogger.Add(LogType.Healed,
                $"{EntityManager.ToPrettyString(args.User):user} healed themselves for {healedTotal:damage} damage");
        }

        _audio.PlayPvs(healing.HealingEndSound, ent, AudioParams.Default.WithVariation(0.125f).WithVolume(1f));

        // Logic to determine whether or not to repeat the healing action
        args.Repeat = IsBodyDamaged((ent, comp), args.User, healing);
        if (!args.Repeat && !dontRepeat)
            _popupSystem.PopupEntity(Loc.GetString("medical-item-finished-using", ("item", args.Used)), ent, args.User, PopupType.Medium);
        args.Handled = true;
    }

    // Shitmed Change End

    private void OnHealingUse(Entity<HealingComponent> entity, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (TryHeal(entity, args.User, args.User, entity.Comp))
            args.Handled = true;
    }

    private void OnHealingAfterInteract(Entity<HealingComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (TryHeal(entity, args.User, args.Target.Value, entity.Comp))
            args.Handled = true;
    }

    private bool TryHeal(EntityUid uid, EntityUid user, EntityUid target, HealingComponent component)
    {
        if (!TryComp<DamageableComponent>(target, out var targetDamage))
            return false;

        if (component.DamageContainers is not null &&
            targetDamage.DamageContainerID is not null &&
            !component.DamageContainers.Contains(targetDamage.DamageContainerID))
        {
            return false;
        }

        if (user != target && !_interactionSystem.InRangeUnobstructed(user, target, popup: true))
            return false;

        if (TryComp<StackComponent>(uid, out var stack) && stack.Count < 1)
            return false;

        // Shitmed Change Start
        var anythingToDo =
            HasDamage((target, targetDamage), component) ||
            (TryComp<BodyComponent>(target, out var bodyComp) && // I'm paranoid, sorry.
             IsBodyDamaged((target, bodyComp), user, component)) ||
            component.ModifyBloodLevel > 0 // Special case if healing item can restore lost blood...
                && TryComp<BloodstreamComponent>(target, out var bloodstream)
                && _solutionContainerSystem.ResolveSolution(target, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution)
                && bloodSolution.Volume < bloodSolution.MaxVolume; // ...and there is lost blood to restore.

        if (!anythingToDo)
        {
            _popupSystem.PopupEntity(Loc.GetString("medical-item-cant-use", ("item", uid)), uid, user);
            return false;
        }
        // Shitmed Change End

        _audio.PlayPvs(component.HealingBeginSound, uid, AudioParams.Default.WithVariation(.125f).WithVolume(1f));

        var isNotSelf = user != target;

        if (isNotSelf)
        {
            var msg = Loc.GetString("medical-item-popup-target", ("user", Identity.Entity(user, EntityManager)), ("item", uid));
            _popupSystem.PopupEntity(msg, target, target, PopupType.Medium);
        }

        var delay = isNotSelf
            ? component.Delay
            : component.Delay * GetScaledHealingPenalty(user, component);

        var doAfterEventArgs =
            new DoAfterArgs(EntityManager, user, delay, new HealingDoAfterEvent(), target, target: target, used: uid)
            {
                // Didn't break on damage as they may be trying to prevent it and
                // not being able to heal your own ticking damage would be frustrating.
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
            };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
        return true;
    }

    /// <summary>
    /// Scales the self-heal penalty based on the amount of damage taken
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public float GetScaledHealingPenalty(EntityUid uid, HealingComponent component)
    {
        var output = component.Delay;
        if (!TryComp<MobThresholdsComponent>(uid, out var mobThreshold))
            return output;
        if (!_mobThresholdSystem.TryGetThresholdForState(uid, MobState.Critical, out var amount, mobThreshold))
            return 1;

        var percentDamage = (float) 1;
        if (TryComp<DamageableComponent>(uid, out var damageable))
            percentDamage = (float) (damageable.TotalDamage / amount);
        else if (TryComp<ConsciousnessComponent>(uid, out var consciousness))
            percentDamage = (float) (consciousness.Threshold / consciousness.Cap - consciousness.Consciousness);
         //basically make it scale from 1 to the multiplier.
        var modifier = percentDamage * (component.SelfHealPenaltyMultiplier - 1) + 1;
        return Math.Max(modifier, 1);
    }
}
