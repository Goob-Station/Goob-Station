// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leeroy <97187620+elthundercloud@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 MisterMecky <mrmecky@hotmail.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 J <billsmith116@gmail.com>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Whatstone <166147148+whatston3@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;

// Shitmed Change
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;
using Robust.Shared.Network;

namespace Content.Shared.Medical.Healing;

public sealed class HealingSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStackSystem _stacks = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    // Shitmed Change
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedTargetingSystem _targetingSystem = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;

    // Goobstation edit
    [Dependency] private readonly INetManager _net = default!;

    // Goobstation start
    private TargetBodyPart[] _partHealingOrder =
        {
            TargetBodyPart.Head,
            TargetBodyPart.Chest,
            TargetBodyPart.Groin,
            TargetBodyPart.LeftArm,
            TargetBodyPart.LeftHand,
            TargetBodyPart.RightArm,
            TargetBodyPart.RightHand,
            TargetBodyPart.LeftLeg,
            TargetBodyPart.LeftFoot,
            TargetBodyPart.RightLeg,
            TargetBodyPart.RightFoot
        };
    // Goobstation end

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HealingComponent, UseInHandEvent>(OnHealingUse);
        SubscribeLocalEvent<HealingComponent, AfterInteractEvent>(OnHealingAfterInteract);
        SubscribeLocalEvent<DamageableComponent, HealingDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<BodyComponent, HealingDoAfterEvent>(OnBodyDoAfter); // Shitmed Change

    }

    private void OnDoAfter(Entity<DamageableComponent> target, ref HealingDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        // Shitmed Change: Consciousness check because some body entities don't have Consciousness
        if (!TryComp(args.Used, out HealingComponent? healing)
            || HasComp<BodyComponent>(target)
            && HasComp<ConsciousnessComponent>(target))
            return;

        if (healing.DamageContainers is not null &&
            target.Comp.DamageContainerID is not null &&
            !healing.DamageContainers.Contains(target.Comp.DamageContainerID.Value))
        {
            return;
        }

        TryComp<BloodstreamComponent>(target, out var bloodstream);

        // Heal some bloodloss damage.
        if (healing.BloodlossModifier != 0 && bloodstream != null)
        {
            var isBleeding = bloodstream.BleedAmount > 0;
            _bloodstreamSystem.TryModifyBleedAmount((target.Owner, bloodstream), healing.BloodlossModifier);
            if (isBleeding != bloodstream.BleedAmount > 0)
            {
                var popup = (args.User == target.Owner)
                    ? Loc.GetString("medical-item-stop-bleeding-self")
                    : Loc.GetString("medical-item-stop-bleeding", ("target", Identity.Entity(target.Owner, EntityManager)));
                _popupSystem.PopupClient(popup, target, args.User);
            }
        }

        // Restores missing blood
        if (healing.ModifyBloodLevel != 0 && bloodstream != null)
            _bloodstreamSystem.TryModifyBloodLevel((target.Owner, bloodstream), -healing.ModifyBloodLevel); // Goobedit

        var healed = _damageable.TryChangeDamage(target.Owner, healing.Damage * _damageable.UniversalTopicalsHealModifier, true, origin: args.Args.User);

        if (healed == null && healing.BloodlossModifier != 0)
            return;

        var total = healed?.GetTotal() ?? FixedPoint2.Zero;

        // Re-verify that we can heal the damage.
        var dontRepeat = false;
        if (TryComp<StackComponent>(args.Used.Value, out var stackComp))
        {
            _stacks.Use(args.Used.Value, 1, stackComp);

            if (_stacks.GetCount(args.Used.Value, stackComp) <= 0)
                dontRepeat = true;
        }
        else
        {
            PredictedQueueDel(args.Used.Value);
        }

        if (target.Owner != args.User)
        {
            _adminLogger.Add(LogType.Healed,
                $"{ToPrettyString(args.User):user} healed {ToPrettyString(target.Owner):target} for {total:damage} damage");
        }
        else
        {
            _adminLogger.Add(LogType.Healed,
                $"{ToPrettyString(args.User):user} healed themselves for {total:damage} damage");
        }

        // Goobstation
        // Only play sound if this is not a body part (body parts are handled by OnBodyDoAfter)
        if (!HasComp<BodyComponent>(target.Owner))
            _audio.PlayPredicted(healing.HealingEndSound, target.Owner, args.User);

        // Logic to determine the whether or not to repeat the healing action
        args.Repeat = HasDamage((args.Used.Value, healing), target) && !dontRepeat;
        args.Handled = true;

        if (!args.Repeat)
        {
            _popupSystem.PopupClient(Loc.GetString("medical-item-finished-using", ("item", args.Used)), target.Owner, args.User);
            return;
        }

        // Update our self heal delay so it shortens as we heal more damage.
        if (args.User == target.Owner)
            args.Args.Delay = healing.Delay * GetScaledHealingPenalty(target.Owner, healing.SelfHealPenaltyMultiplier);
    }

    private bool HasDamage(Entity<HealingComponent> healing, Entity<DamageableComponent> target)
    {
        var damageableDict = target.Comp.Damage.DamageDict;
        var healingDict = healing.Comp.Damage.DamageDict;
        foreach (var type in healingDict)
        {
            if (damageableDict[type.Key].Value > 0)
            {
                return true;
            }
        }

        if (TryComp<BloodstreamComponent>(target, out var bloodstream))
        {
            // Is ent missing blood that we can restore?
            if (healing.Comp.ModifyBloodLevel > 0
                && _solutionContainerSystem.ResolveSolution(target.Owner, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution)
                && bloodSolution.Volume < bloodSolution.MaxVolume)
            {
                return true;
            }

            // Is ent bleeding and can we stop it?
            if (healing.Comp.BloodlossModifier < 0 && bloodstream.BleedAmount > 0)
            {
                return true;
            }
        }

        return false;
    }

    // Shitmed Change Start

    private string? GetDamageGroupByType(string id)
        => (from @group in _prototypes.EnumeratePrototypes<DamageGroupPrototype>() where @group.DamageTypes.Contains(id) select @group.ID).FirstOrDefault();


    // Goobstation start
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
        if (user is null && targetedPart is null) // no limb can be targeted at all
            return false;

        if (user is not null)
        {
            if (!TryComp<TargetingComponent>(user, out var targeting))
                return false;

            var (partType, symmetry) = _bodySystem.ConvertTargetBodyPart(targeting.Target);
            var targetedBodyPart = _bodySystem.GetBodyChildrenOfType(target, partType, target, symmetry).ToList().FirstOrNull();

            if (targetedBodyPart is not null && targetedPart is null)
                targetedPart = targetedBodyPart.Value.Id;
        }

        if (targetedPart == null
            || !TryComp(targetedPart, out DamageableComponent? damageable))
        {
            if (user is not null)
                // Goobstation Predicted --> Client
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
            if (!TryComp<BleedInflicterComponent>(wound, out var bleeds) || !bleeds.IsBleeding)
                continue;

            return true;
        }
        // Goobstation end

        return false;
    }

    /// <summary>
    ///     This function tries to return the first limb that has one of the damage type we are trying to heal
    ///     Returns true or false if next damaged part exists.
    /// </summary>
    public bool TryGetNextDamagedPart(EntityUid ent, HealingComponent healing, out EntityUid? part) // Goob edit: private => public, used in RepairableSystems.cs
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

        // Goobstation - commented out as it is no longer needed due to newer topical application logic
        // Goobstation start
        /*if (!IsBodyDamaged((ent, comp), null, healing, targetedWoundable))                          // Check if there is anything to heal on the initial limb target
            if (TryGetNextDamagedPart(ent, healing, out var limbTemp) && limbTemp is not null)      // If not then get the next limb to heal
                targetedWoundable = limbTemp.Value;*/
        // Goobstation end

        if (targetedWoundable == EntityUid.Invalid)
        {
            // Goobstation - predicted --> client
            _popupSystem.PopupClient(
                Loc.GetString("medical-item-cant-use", ("item", args.Used)),
                ent,
                args.User,
                PopupType.MediumCaution);
            return;
        }

        if (!TryComp<WoundableComponent>(targetedWoundable, out var woundableComp)
            || !TryComp<DamageableComponent>(targetedWoundable, out var damageableComp))
            return;

        var healedBleed = false;
        var canHeal = true;
        var healedTotal = new DamageSpecifier(); // Goobstation
        FixedPoint2 modifiedBleedStopAbility = 0;
        // Heal some bleeds
        bool healedBleedLevel = false;
        if (healing.BloodlossModifier != 0)
        {
            // Goobstation start
            var bleedBefore = 0.0;
            if (TryComp<BloodstreamComponent>(ent, out var bloodstream))
                bleedBefore = bloodstream.BleedAmountFromWounds + bloodstream.BleedAmountNotFromWounds;
            healedBleed = bleedBefore > 0.0;
            _wounds.TryHealBleedingWounds(targetedWoundable, healing.BloodlossModifier, out modifiedBleedStopAbility, woundableComp);
            if (healing.BloodlossModifier + modifiedBleedStopAbility < 0.0)
                _bloodstreamSystem.TryModifyBleedAmount(ent, (healing.BloodlossModifier + modifiedBleedStopAbility).Float()); // Use the leftover bleed heal
            if (healedBleed)
                _popupSystem.PopupClient(bleedBefore + healing.BloodlossModifier <= 0.0
                        ? Loc.GetString("rebell-medical-item-stop-bleeding-fully")
                        : Loc.GetString("rebell-medical-item-stop-bleeding-partially"),
                    ent,
                    args.User);
            // Goobstation end
        }

        if (healing.ModifyBloodLevel != 0)
            healedBleedLevel = _bloodstreamSystem.TryModifyBloodLevel(ent, -healing.ModifyBloodLevel);

        //healedBleed = healedBleedWound || healedBleedLevel;

        // Goobstation start
        // Create parts to go over queue: targetted part -> head -> torso -> groin -> everything else
        // Iterate over the parts in the predefined order until we run out of parts or run out of healing
        var woundablesQueue = new Queue<EntityUid>();
        woundablesQueue.Enqueue(targetedWoundable);
        for (var i = 0; i < _partHealingOrder.Length; i++)
        {
            var (partType, symmetry) = _bodySystem.ConvertTargetBodyPart(_partHealingOrder[i]);
            var targetedBodyPart = _bodySystem.GetBodyChildrenOfType(ent, partType, comp, symmetry).ToList().FirstOrDefault();
            if (targetedBodyPart.Id == targetedWoundable)
                continue;
            woundablesQueue.Enqueue(targetedBodyPart.Id);
        }

        var leftoverHealAndTrauma = false;
        var leftoverHealAndBleed = false;
        var healingLeft = healing.Damage * _damageable.UniversalTopicalsHealModifier;
        while (woundablesQueue.Count > 0 && healingLeft.GetTotal() < 0.0)
        {
            canHeal = true;
            targetedWoundable = woundablesQueue.Dequeue();
            if (!TryComp<WoundableComponent>(targetedWoundable, out var woundableComp2))
                continue;
            if (TraumaSystem.TraumasBlockingHealing.Any(traumaType => _trauma.HasWoundableTrauma(targetedWoundable, traumaType, woundableComp2, false)))
            {
                canHeal = false;

                if (!healedBleedLevel)
                {
                    leftoverHealAndTrauma = true;
                    continue;
                }
            }

            if (canHeal)
            {
                if (healing.BloodlossModifier == 0 && healing.ModifyBloodLevel >= 0 && woundableComp2.Bleeds > 0)  // If the healing item has no bleeding heals, and its bleeding, we raise the alert. Goobstation edit
                {
                    leftoverHealAndBleed = true;
                    continue;
                }

                var damageChanged = _damageable.TryChangeDamage(targetedWoundable, healingLeft, true, origin: args.User, ignoreBlockers: healedBleed || healing.BloodlossModifier == 0); // GOOBEDIT

                if (damageChanged is not null)
                {
                    healedTotal += -damageChanged;
                    healingLeft += -damageChanged;
                }
            }
        }

        var isAnyTypeFullyConsumed = healingLeft.DamageDict.Any(d => d.Value == 0);

        if (!healedBleed && !isAnyTypeFullyConsumed && (leftoverHealAndTrauma || leftoverHealAndBleed))
        {
            if (leftoverHealAndTrauma)
                _popupSystem.PopupClient(Loc.GetString("medical-item-requires-surgery-rebell", ("target", ent)), ent, args.User, PopupType.MediumCaution);
            else if (leftoverHealAndBleed) // the else is because would like to not pop both the popups at once, priority goes to the trauma popup
                _popupSystem.PopupClient(Loc.GetString("medical-item-cant-use-rebell", ("target", ent)), ent, args.User);
            return;
        }
        // Goobstation end

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

        if (ent != args.User)
        {
            _adminLogger.Add(LogType.Healed,
                $"{EntityManager.ToPrettyString(args.User):user} healed {EntityManager.ToPrettyString(ent):target} for {healedTotal.GetTotal():damage} damage"); // Goobstation
        }
        else
        {
            _adminLogger.Add(LogType.Healed,
                $"{EntityManager.ToPrettyString(args.User):user} healed themselves for {healedTotal.GetTotal():damage} damage"); // Goobstation
        }
        _audio.PlayPredicted(healing.HealingEndSound, ent, ent, AudioParams.Default.WithVariation(0.125f).WithVolume(1f)); // Goob edit

        // Logic to determine whether or not to repeat the healing action
        args.Repeat = IsAnythingToHeal(args.User, ent, (args.Used.Value, healing)); // GOOBEDIT
        args.Handled = true;

        if (args.Repeat || dontRepeat)
            return;

        if (modifiedBleedStopAbility != -healing.BloodlossModifier)
            // Goobstation predicted --> client
            _popupSystem.PopupClient(Loc.GetString("medical-item-finished-using", ("item", args.Used)), ent, args.User, PopupType.Medium);
    }

    // Shitmed Change End
    private void OnHealingUse(Entity<HealingComponent> healing, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (TryHeal(healing, args.User, args.User))
            args.Handled = true;
    }

    private void OnHealingAfterInteract(Entity<HealingComponent> healing, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (TryHeal(healing, args.Target.Value, args.User))
            args.Handled = true;
    }

    // Goobstation start
    private bool IsAnythingToHeal(EntityUid user, EntityUid target, Entity<HealingComponent> healing)
    {
        if (!TryComp<DamageableComponent>(target, out var targetDamage))
            return false;

        return HasDamage(healing, (target, targetDamage)) ||
            TryComp<BodyComponent>(target, out var bodyComp) && // I'm paranoid, sorry.
            IsBodyDamaged((target, bodyComp), user, healing.Comp) ||
            healing.Comp.ModifyBloodLevel > 0 // Special case if healing item can restore lost blood...
                && TryComp<BloodstreamComponent>(target, out var bloodstream)
                && _solutionContainerSystem.ResolveSolution(target, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution)
                && bloodSolution.Volume < bloodSolution.MaxVolume;
    }
    // Goobstation end

    private bool TryHeal(Entity<HealingComponent> healing, Entity<DamageableComponent?> target, EntityUid user)
    {
        if (!Resolve(target, ref target.Comp, false))
            return false;

        if (healing.Comp.DamageContainers is not null &&
            target.Comp.DamageContainerID is not null &&
            !healing.Comp.DamageContainers.Contains(target.Comp.DamageContainerID.Value))
        {
            return false;
        }

        if (user != target.Owner && !_interactionSystem.InRangeUnobstructed(user, target.Owner, popup: true))
            return false;

        if (TryComp<StackComponent>(healing, out var stack) && stack.Count < 1)
            return false;

        // Shitmed Change Start
        var anythingToDo =
            HasDamage(healing, (target.Owner, target.Comp)) ||
            TryComp<BodyComponent>(target, out var bodyComp) && // I'm paranoid, sorry.
            IsBodyDamaged((target, bodyComp), user, healing.Comp) ||
            healing.Comp.ModifyBloodLevel < 0 // Special case if healing item can restore lost blood... Goobstation edit
                && TryComp<BloodstreamComponent>(target, out var bloodstream)
                && _solutionContainerSystem.ResolveSolution(target.Owner, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution)
                && bloodSolution.Volume < bloodSolution.MaxVolume; // ...and there is lost blood to restore.

        if (!anythingToDo)
        {
            _popupSystem.PopupClient(Loc.GetString("medical-item-cant-use", ("item", healing.Owner)), healing, user);
            return false;
        }
        // Shitmed Change End
            // Goobstation Moved - to avoid audio spam
            //_audio.PlayPredicted(healing.Comp.HealingBeginSound, healing, user);

        var isNotSelf = user != target.Owner;

        if (isNotSelf)
        {
            // Show this to the target
            // Goobstation predicted --> client
            var msg = Loc.GetString("medical-item-popup-target", ("user", Identity.Entity(user, EntityManager)), ("item", healing.Owner));
            _popupSystem.PopupClient(msg, target, target, PopupType.Medium);
        }

        var delay = isNotSelf
            ? healing.Comp.Delay
            : healing.Comp.Delay * GetScaledHealingPenalty(target.Owner, healing.Comp.SelfHealPenaltyMultiplier);

        // Play sound when starting the healing action
        // Goobstation
        _audio.PlayPredicted(healing.Comp.HealingBeginSound, target, user);

        var doAfterEventArgs =
            new DoAfterArgs(EntityManager, user, delay, new HealingDoAfterEvent(), target, target: target, used: healing)
            {
                // Didn't break on damage as they may be trying to prevent it and
                // not being able to heal your own ticking damage would be frustrating.
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = false
            };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
        return true;
    }

    /// <summary>
    /// Scales the self-heal penalty based on the amount of damage taken
    /// </summary>
    /// <param name="ent">Entity we're healing</param>
    /// <param name="mod">Maximum modifier we can have.</param>
    /// <returns>Modifier we multiply our healing time by</returns>
    public float GetScaledHealingPenalty(Entity<DamageableComponent?, MobThresholdsComponent?> ent, float mod)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2, false))
            return mod;

        if (!_mobThresholdSystem.TryGetThresholdForState(ent, MobState.Critical, out var amount, ent.Comp2))
            return 1;

        var percentDamage = (float)(ent.Comp1.TotalDamage / amount);
        if (TryComp<ConsciousnessComponent>(ent.Owner, out var consciousness))
            percentDamage *= (float) (consciousness.Threshold / consciousness.Cap - consciousness.Consciousness);

        //basically make it scale from 1 to the multiplier.
        var output = percentDamage * (mod - 1) + 1;
        return Math.Max(output, 1);
    }
}
