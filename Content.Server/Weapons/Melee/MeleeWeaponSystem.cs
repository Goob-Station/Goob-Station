// SPDX-FileCopyrightText: 2022 CommieFlowers <rasmus.cedergren@hotmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 rolfero <45628623+rolfero@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 HerCoyote23 <131214189+HerCoyote23@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 I.K <45953835+notquitehadouken@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 notquitehadouken <1isthisameme>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Bixkitts <72874643+Bixkitts@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Callmore <22885888+Callmore@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Джексон Миссиссиппи <tripwiregamer@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Eagle <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 vanx <61917534+Vaaankas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.CombatMode.Disarm;
using Content.Server.Movement.Systems;
using Content.Shared.Actions.Events;
using Content.Shared.Administration.Components;
using Content.Shared.CombatMode;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Hands.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Speech.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.MartialArts;
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared.Coordinates;
using Content.Shared.Item;
using Content.Shared.Throwing;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;

namespace Content.Server.Weapons.Melee;

public sealed class MeleeWeaponSystem : SharedMeleeWeaponSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly DamageExamineSystem _damageExamine = default!;
    [Dependency] private readonly LagCompensationSystem _lag = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!; // Goob - Shove Rework
    [Dependency] private readonly INetConfigurationManager _config = default!; // Goob - Shove Rework
    [Dependency] private readonly SharedTransformSystem _transform = default!; // Goob - Shove Rework

    //Goob - Shove
    private float _shoveRange;
    private float _shoveSpeed;
    private float _shoveMass;
    //Goob - Shove

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MeleeSpeechComponent, MeleeHitEvent>(OnSpeechHit);
        SubscribeLocalEvent<MeleeWeaponComponent, DamageExamineEvent>(OnMeleeExamineDamage);
        Subs.CVar(_config, GoobCVars.ShoveRange, SetShoveRange, true);
        Subs.CVar(_config, GoobCVars.ShoveSpeed, SetShoveSpeed, true);
        Subs.CVar(_config, GoobCVars.ShoveMassFactor, SetShoveMass, true);
    }

    // Goobstation - Shove
    private void SetShoveRange(float value)
    {
        _shoveRange = value;
    }

    private void SetShoveSpeed(float value)
    {
        _shoveSpeed = value;
    }

    private void SetShoveMass(float value)
    {
        _shoveMass = value;
    }
    //Goob - Shove

    private void OnMeleeExamineDamage(EntityUid uid, MeleeWeaponComponent component, ref DamageExamineEvent args)
    {
        if (component.Hidden)
            return;

        var damageSpec = GetDamage(uid, args.User, component);

        if (damageSpec.Empty)
            return;

        _damageExamine.AddDamageExamine(args.Message, Damageable.ApplyUniversalAllModifiers(damageSpec), Loc.GetString("damage-melee"));
    }

    protected override bool ArcRaySuccessful(EntityUid targetUid,
        Vector2 position,
        Angle angle,
        Angle arcWidth,
        float range,
        MapId mapId,
        EntityUid ignore,
        ICommonSession? session)
    {
        // Originally the client didn't predict damage effects so you'd intuit some level of how far
        // in the future you'd need to predict, but then there was a lot of complaining like "why would you add artifical delay" as if ping is a choice.
        // Now damage effects are predicted but for wide attacks it differs significantly from client and server so your game could be lying to you on hits.
        // This isn't fair in the slightest because it makes ping a huge advantage and this would be a hidden system.
        // Now the client tells us what they hit and we validate if it's plausible.

        // Even if the client is sending entities they shouldn't be able to hit:
        // A) Wide-damage is split anyway
        // B) We run the same validation we do for click attacks.

        // Could also check the arc though future effort + if they're aimbotting it's not really going to make a difference.

        // (This runs lagcomp internally and is what clickattacks use)
        if (!Interaction.InRangeUnobstructed(ignore, targetUid, range + 0.1f, overlapCheck: false))
            return false;

        // TODO: Check arc though due to the aforementioned aimbot + damage split comments it's less important.
        return true;
    }

    protected override bool DoDisarm(EntityUid user,
        DisarmAttackEvent ev,
        EntityUid meleeUid,
        MeleeWeaponComponent component,
        ICommonSession? session) // Goobstation - Shove Rework
    {
        if (!base.DoDisarm(user, ev, meleeUid, component, session))
            return false;

        var target = GetEntity(ev.Target!.Value);

        EntityUid? inTargetHand = null;

        if (!TryComp<CombatModeComponent>(user, out var combatMode))
            return false;

        PhysicalShove(user, target);
        Interaction.DoContactInteraction(user, target);

        if (_mobState.IsIncapacitated(target))
            return true;

        if (!TryComp<PhysicsComponent>(target, out var targetPhysicsComponent))
            return false;

        if (!TryComp<HandsComponent>(target, out var targetHandsComponent))
        {
            if (!TryComp<StatusEffectsComponent>(target, out var status) ||
                !status.AllowedEffects.Contains("KnockedDown"))
            {
                return true;
            }
        }

        if (targetHandsComponent?.ActiveHand is { IsEmpty: false })
        {
            inTargetHand = targetHandsComponent.ActiveHand.HeldEntity!.Value;
        }

        Interaction.DoContactInteraction(user, target);

        var comboEv = new ComboAttackPerformedEvent(user, target, meleeUid, ComboAttackType.Disarm);
        RaiseLocalEvent(user, comboEv);

        var attemptEvent = new DisarmAttemptEvent(target, user, inTargetHand);
        if (inTargetHand != null)
        {
            RaiseLocalEvent(inTargetHand.Value, attemptEvent);
        }

        RaiseLocalEvent(target, attemptEvent);

        if (attemptEvent.Cancelled)
            return true;

        var chance = CalculateDisarmChance(user, target, inTargetHand, combatMode);

        _audio.PlayPvs(combatMode.DisarmSuccessSound,
            user,
            AudioParams.Default.WithVariation(0.025f).WithVolume(5f));
        AdminLogger.Add(LogType.DisarmedAction,
            $"{ToPrettyString(user):user} used disarm on {ToPrettyString(target):target}");

        var staminaDamage = CalculateShoveStaminaDamage(user, target);

        var eventArgs = new DisarmedEvent
        {
            Target = target, Source = user, DisarmProbability = chance, StaminaDamage = staminaDamage,
        };
        RaiseLocalEvent(target, eventArgs);

        if (!eventArgs.Handled)
        {
            ShoveOrDisarmPopup(false);
            return true;
        }

        ShoveOrDisarmPopup(true);
        _audio.PlayPvs(combatMode.DisarmSuccessSound,
            user,
            AudioParams.Default.WithVariation(0.025f).WithVolume(5f));
        AdminLogger.Add(LogType.DisarmedAction,
            $"{ToPrettyString(user):user} used a shove on {ToPrettyString(target):target}");

        return true;

        // Goob - Shove Rework edit (moved to function)
        void ShoveOrDisarmPopup(bool disarm)
        {
            var filterOther = Filter.PvsExcept(user, entityManager: EntityManager);
            var msgPrefix = "disarm-action-";

            if (!disarm)
                msgPrefix = "disarm-action-shove-";

            var msgOther = Loc.GetString(
                msgPrefix + "popup-message-other-clients",
                ("performerName", Identity.Entity(user, EntityManager)),
                ("targetName", Identity.Entity(target, EntityManager)));

            var msgUser = Loc.GetString(msgPrefix + "popup-message-cursor", ("targetName", Identity.Entity(target, EntityManager)));

            PopupSystem.PopupEntity(msgOther, user, filterOther, true);
            PopupSystem.PopupEntity(msgUser, target, user);
        }
    }

    protected override bool InRange(EntityUid user, EntityUid target, float range, ICommonSession? session)
    {
        EntityCoordinates targetCoordinates;
        Angle targetLocalAngle;

        if (session is { } pSession)
        {
            (targetCoordinates, targetLocalAngle) = _lag.GetCoordinatesAngle(target, pSession);
            return Interaction.InRangeUnobstructed(user, target, targetCoordinates, targetLocalAngle, range, overlapCheck: false);
        }

        return Interaction.InRangeUnobstructed(user, target, range);
    }

    private void PhysicalShove(EntityUid user, EntityUid target)
    {
        var force = _shoveRange * _contests.MassContest(user, target, rangeFactor: _shoveMass);

        var userPos = _transform.ToMapCoordinates(user.ToCoordinates()).Position;
        var targetPos = _transform.ToMapCoordinates(target.ToCoordinates()).Position;
        var pushVector = (targetPos - userPos).Normalized() * force;

        var animated = false;
        var throwInAir = false;

        if (HasComp<ItemComponent>(target)) // Throw items instead of shoving
        {
            animated = true;
            throwInAir = true;
        }

        _throwing.TryThrow(target, pushVector, force * _shoveSpeed, animated: animated, throwInAir: throwInAir);
    }

    protected override void DoDamageEffect(List<EntityUid> targets, EntityUid? user, TransformComponent targetXform)
    {
        var filter = Filter.Pvs(targetXform.Coordinates, entityMan: EntityManager).RemoveWhereAttachedEntity(o => o == user);
        _color.RaiseEffect(Color.Red, targets, filter);
    }

    private float CalculateDisarmChance(EntityUid disarmer, EntityUid disarmed, EntityUid? inTargetHand, CombatModeComponent disarmerComp)
    {
        if (HasComp<DisarmProneComponent>(disarmer))
            return 1.0f;

        if (HasComp<DisarmProneComponent>(disarmed))
            return 0.0f;

        var chance = 1 - disarmerComp.BaseDisarmFailChance;

        // Goob - Shove Rework disarm based on health & stamina
        chance *= Math.Clamp(
            _contests.StaminaContest(disarmer, disarmed)
            * _contests.HealthContest(disarmer, disarmed),
            0f,
            1f);

        if (inTargetHand != null && TryComp<DisarmMalusComponent>(inTargetHand, out var malus))
            chance *= 1 - malus.Malus; // Goob - Shove Rework edit

        if (TryComp<ShovingComponent>(disarmer, out var shoving))
            chance *= 1 + shoving.DisarmBonus; // Goob - Shove Rework edit

        return chance;

    }

    // Goob - Shove Rework shove stamina damage based on mass
    private float CalculateShoveStaminaDamage(EntityUid disarmer, EntityUid disarmed)
    {
        var baseStaminaDamage = TryComp<ShovingComponent>(disarmer, out var shoving) ? shoving.StaminaDamage : ShovingComponent.DefaultStaminaDamage;

        return
            baseStaminaDamage
            * _contests.MassContest(disarmer, disarmed, false, 4f);
    }

    public override void DoLunge(EntityUid user, EntityUid weapon, Angle angle, Vector2 localPos, string? animation, Angle spriteRotation, bool flipAnimation, bool predicted = true)
    {
        Filter filter;

        if (predicted)
        {
            filter = Filter.PvsExcept(user, entityManager: EntityManager);
        }
        else
        {
            filter = Filter.Pvs(user, entityManager: EntityManager);
        }

        RaiseNetworkEvent(new MeleeLungeEvent(GetNetEntity(user), GetNetEntity(weapon), angle, localPos, animation, spriteRotation, flipAnimation), filter);
    }

    private void OnSpeechHit(EntityUid owner, MeleeSpeechComponent comp, MeleeHitEvent args)
    {
        if (!args.IsHit ||
        !args.HitEntities.Any())
        {
            return;
        }

        if (comp.Battlecry != null)//If the battlecry is set to empty, doesn't speak
        {
            _chat.TrySendInGameICMessage(args.User, comp.Battlecry, InGameICChatType.Speak, true, true, checkRadioPrefix: false);  //Speech that isn't sent to chat or adminlogs
        }

    }
}