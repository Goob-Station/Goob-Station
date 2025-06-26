// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 FoxxoTrystan <45297731+FoxxoTrystan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 HS <81934438+HolySSSS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pspritechologist <naaronn@gmail.com>
// SPDX-FileCopyrightText: 2024 Rouge2t7 <81053047+Sarahon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SkaldetSkaeg <impotekh@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Truoizys <153248924+Truoizys@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TsjipTsjip <19798667+TsjipTsjip@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ubaser <134914314+UbaserB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 Арт <123451459+JustArt1m@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Coolsurf6 <coolsurf24@yahoo.com.au>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Emoting;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Pointing;
using Content.Shared.Popups;
using Content.Shared.Slippery;
using Content.Shared.Sound;
using Content.Shared.Sound.Components;
using Content.Shared.Speech;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Traits.Assorted;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.Bed.Sleep;

public sealed partial class SleepingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly BlindableSystem _blindableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedEmitSoundSystem _emitSound = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;

    public static readonly EntProtoId SleepActionId = "ActionSleep";
    public static readonly EntProtoId WakeActionId = "ActionWake";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionsContainerComponent, SleepActionEvent>(OnBedSleepAction);

        SubscribeLocalEvent<MobStateComponent, SleepStateChangedEvent>(OnSleepStateChanged);
        SubscribeLocalEvent<MobStateComponent, WakeActionEvent>(OnWakeAction);
        SubscribeLocalEvent<MobStateComponent, SleepActionEvent>(OnSleepAction);

        SubscribeLocalEvent<SleepingComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<SleepingComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<SleepingComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SleepingComponent, SpeakAttemptEvent>(OnSpeakAttempt);
        SubscribeLocalEvent<SleepingComponent, CanSeeAttemptEvent>(OnSeeAttempt);
        SubscribeLocalEvent<SleepingComponent, PointAttemptEvent>(OnPointAttempt);
        SubscribeLocalEvent<SleepingComponent, SlipAttemptEvent>(OnSlip);
        SubscribeLocalEvent<SleepingComponent, ConsciousAttemptEvent>(OnConsciousAttempt);
        SubscribeLocalEvent<SleepingComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SleepingComponent, GetVerbsEvent<AlternativeVerb>>(AddWakeVerb);
        SubscribeLocalEvent<SleepingComponent, InteractHandEvent>(OnInteractHand);

        SubscribeLocalEvent<ForcedSleepingComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SleepingComponent, UnbuckleAttemptEvent>(OnUnbuckleAttempt);
        SubscribeLocalEvent<SleepingComponent, EmoteAttemptEvent>(OnEmoteAttempt);

        SubscribeLocalEvent<SleepingComponent, BeforeForceSayEvent>(OnChangeForceSay, after: new []{typeof(PainNumbnessSystem)});
    }

    private void OnUnbuckleAttempt(Entity<SleepingComponent> ent, ref UnbuckleAttemptEvent args)
    {
        // TODO is this necessary?
        // Shouldn't the interaction have already been blocked by a general interaction check?
        if (ent.Owner == args.User)
            args.Cancelled = true;
    }

    private void OnBedSleepAction(Entity<ActionsContainerComponent> ent, ref SleepActionEvent args)
    {
        TrySleeping(args.Performer);
    }

    private void OnWakeAction(Entity<MobStateComponent> ent, ref WakeActionEvent args)
    {
        if (TryWakeWithCooldown(ent.Owner))
            args.Handled = true;
    }

    private void OnSleepAction(Entity<MobStateComponent> ent, ref SleepActionEvent args)
    {
        TrySleeping((ent, ent.Comp));
    }

    /// <summary>
    /// when sleeping component is added or removed, we do some stuff with other components.
    /// </summary>
    private void OnSleepStateChanged(Entity<MobStateComponent> ent, ref SleepStateChangedEvent args)
    {
        if (args.FellAsleep)
        {
            // Expiring status effects would remove the components needed for sleeping
            _statusEffectsSystem.TryRemoveStatusEffect(ent.Owner, "Stun");
            _statusEffectsSystem.TryRemoveStatusEffect(ent.Owner, "KnockedDown");

            EnsureComp<StunnedComponent>(ent);
            EnsureComp<KnockedDownComponent>(ent);

            if (TryComp<SleepEmitSoundComponent>(ent, out var sleepSound))
            {
                var emitSound = EnsureComp<SpamEmitSoundComponent>(ent);
                if (HasComp<SnoringComponent>(ent))
                {
                    emitSound.Sound = sleepSound.Snore;
                }
                emitSound.MinInterval = sleepSound.Interval;
                emitSound.MaxInterval = sleepSound.MaxInterval;
                emitSound.PopUp = sleepSound.PopUp;
                Dirty(ent.Owner, emitSound);
            }

            return;
        }

        RemComp<StunnedComponent>(ent);
        RemComp<KnockedDownComponent>(ent);
        RemComp<SpamEmitSoundComponent>(ent);
    }

    private void OnMapInit(Entity<SleepingComponent> ent, ref MapInitEvent args)
    {
        var ev = new SleepStateChangedEvent(true);
        RaiseLocalEvent(ent, ref ev);
        _blindableSystem.UpdateIsBlind(ent.Owner);
        _actionsSystem.AddAction(ent, ref ent.Comp.WakeAction, WakeActionId, ent);
    }

    private void OnSpeakAttempt(Entity<SleepingComponent> ent, ref SpeakAttemptEvent args)
    {
        // TODO reduce duplication of this behavior with MobStateSystem somehow
        if (HasComp<AllowNextCritSpeechComponent>(ent))
        {
            RemCompDeferred<AllowNextCritSpeechComponent>(ent);
            return;
        }

        args.Cancel();
    }

    private void OnSeeAttempt(Entity<SleepingComponent> ent, ref CanSeeAttemptEvent args)
    {
        if (ent.Comp.LifeStage <= ComponentLifeStage.Running)
            args.Cancel();
    }

    private void OnPointAttempt(Entity<SleepingComponent> ent, ref PointAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnSlip(Entity<SleepingComponent> ent, ref SlipAttemptEvent args)
    {
        args.NoSlip = true;
    }

    private void OnConsciousAttempt(Entity<SleepingComponent> ent, ref ConsciousAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnExamined(Entity<SleepingComponent> ent, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
        {
            args.PushMarkup(Loc.GetString("sleep-examined", ("target", Identity.Entity(ent, EntityManager))));
        }
    }

    private void AddWakeVerb(Entity<SleepingComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var target = args.Target;
        var user = args.User;
        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                TryWakeWithCooldown((ent, ent.Comp), user: user);
            },
            Text = Loc.GetString("action-name-wake"),
            Priority = 2
        };

        args.Verbs.Add(verb);
    }

    /// <summary>
    /// When you click on a sleeping person with an empty hand, try to wake them.
    /// </summary>
    private void OnInteractHand(Entity<SleepingComponent> ent, ref InteractHandEvent args)
    {
        args.Handled = true;

        TryWakeWithCooldown((ent, ent.Comp), args.User);
    }

    /// <summary>
    /// Wake up on taking an instance of damage at least the value of WakeThreshold.
    /// </summary>
    private void OnDamageChanged(Entity<SleepingComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        /* Shitmed Change Start - Surgery needs this, sorry! If the nocturine gamers get too feisty
        I'll probably just increase the threshold */

        if (args.DamageDelta.GetTotal() >= ent.Comp.WakeThreshold
            && !HasComp<ForcedSleepingComponent>(ent))
            TryWaking((ent, ent.Comp));

        // Shitmed Change End
    }

    /// <summary>
    /// In crit, we wake up if we are not being forced to sleep.
    /// And, you can't sleep when dead...
    /// </summary>
    private void OnMobStateChanged(Entity<SleepingComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            RemComp<SpamEmitSoundComponent>(ent);
            RemComp<SleepingComponent>(ent);
            return;
        }
        if (TryComp<SpamEmitSoundComponent>(ent, out var spam))
            _emitSound.SetEnabled((ent, spam), args.NewMobState == MobState.Alive);
    }

    private void OnInit(Entity<ForcedSleepingComponent> ent, ref ComponentInit args)
    {
        TrySleeping(ent.Owner);
    }

    private void Wake(Entity<SleepingComponent> ent)
    {
        RemComp<SleepingComponent>(ent);
        _actionsSystem.RemoveAction(ent, ent.Comp.WakeAction);

        var ev = new SleepStateChangedEvent(false);
        RaiseLocalEvent(ent, ref ev);

        _blindableSystem.UpdateIsBlind(ent.Owner);
    }

    /// <summary>
    /// Try sleeping. Only mobs can sleep.
    /// </summary>
    public bool TrySleeping(Entity<MobStateComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, logMissing: false))
            return false;

        var tryingToSleepEvent = new TryingToSleepEvent(ent);
        RaiseLocalEvent(ent, ref tryingToSleepEvent);
        if (tryingToSleepEvent.Cancelled)
            return false;

        EnsureComp<SleepingComponent>(ent);
        return true;
    }

    /// <summary>
    /// Tries to wake up <paramref name="ent"/>, with a cooldown between attempts to prevent spam.
    /// </summary>
    public bool TryWakeWithCooldown(Entity<SleepingComponent?> ent, EntityUid? user = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        var curTime = _gameTiming.CurTime;

        if (curTime < ent.Comp.CooldownEnd)
            return false;

        ent.Comp.CooldownEnd = curTime + ent.Comp.Cooldown;
        Dirty(ent, ent.Comp);
        return TryWaking(ent, user: user);
    }

    /// <summary>
    /// Try to wake up <paramref name="ent"/>.
    /// </summary>
    public bool TryWaking(Entity<SleepingComponent?> ent, bool force = false, EntityUid? user = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!force && HasComp<ForcedSleepingComponent>(ent))
        {
            if (user != null)
            {
                _audio.PlayPredicted(ent.Comp.WakeAttemptSound, ent, user);
                _popupSystem.PopupClient(Loc.GetString("wake-other-failure", ("target", Identity.Entity(ent, EntityManager))), ent, user, PopupType.SmallCaution);
            }
            return false;
        }

        if (user != null)
        {
            _audio.PlayPredicted(ent.Comp.WakeAttemptSound, ent, user);
            _popupSystem.PopupClient(Loc.GetString("wake-other-success", ("target", Identity.Entity(ent, EntityManager))), ent, user);
        }

        Wake((ent, ent.Comp));
        return true;
    }

    /// <summary>
    /// Prevents the use of emote actions while sleeping
    /// </summary>
    public void OnEmoteAttempt(Entity<SleepingComponent> ent, ref EmoteAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnChangeForceSay(Entity<SleepingComponent> ent, ref BeforeForceSayEvent args)
    {
        args.Prefix = ent.Comp.ForceSaySleepDataset;
    }
}


public sealed partial class SleepActionEvent : InstantActionEvent;

public sealed partial class WakeActionEvent : InstantActionEvent;

/// <summary>
/// Raised on an entity when they fall asleep or wake up.
/// </summary>
[ByRefEvent]
public record struct SleepStateChangedEvent(bool FellAsleep);