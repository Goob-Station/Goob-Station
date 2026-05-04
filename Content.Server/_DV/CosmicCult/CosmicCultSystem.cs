// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 loltart <lo1tartyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Server._DV.CosmicCult.Components;
using Content.Server._EinsteinEngines.Radio; // Goobstation - Shitchap
using Content.Server.Actions;
using Content.Server.AlertLevel;
using Content.Server.Atmos.Components;
using Content.Server.Audio;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Events;
using Content.Server.Objectives.Components;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.Radio;
using Content.Shared.Radio;
using Content.Server.Station.Systems;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Content.Shared.Eye;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Speech.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.EntitySerialization;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Radio.Components;

namespace Content.Server._DV.CosmicCult;

public sealed partial class CosmicCultSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AlertLevelSystem _alert = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly CosmicCorruptingSystem _corrupting = default!;
    [Dependency] private readonly CosmicCultRuleSystem _cultRule = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MonumentSystem _monument = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedPointLightSystem _lights = default!;

    private readonly ResPath _mapPath = new("Maps/_DV/Nonstations/cosmicvoid.yml");
    private static readonly EntProtoId CosmicEchoVfx = "CosmicEchoVfx";
    private static readonly ProtoId<StatusEffectPrototype> EntropicDegen = "EntropicDegen";
    private static readonly ProtoId<RadioChannelPrototype> CosmicRadio = "CosmicRadio";
    private static readonly ProtoId<StatusEffectPrototype> EntropicDegenNonCultist = "EntropicDegenNonCultist"; // Goobstation change. For non-cultist equipment debuff

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);

        SubscribeLocalEvent<CosmicCultComponent, ComponentInit>(OnStartCultist);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentInit>(OnStartCultLead);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentShutdown>(OnCultLeadShutdown);
        SubscribeLocalEvent<CosmicCultComponent, GetVisMaskEvent>(OnGetVisMask);

        SubscribeLocalEvent<CosmicEquipmentComponent, GotEquippedEvent>(OnGotCosmicItemEquipped);
        SubscribeLocalEvent<CosmicEquipmentComponent, GotUnequippedEvent>(OnGotCosmicItemUnequipped);
        SubscribeLocalEvent<CosmicEquipmentComponent, GotEquippedHandEvent>(OnGotHeld);
        SubscribeLocalEvent<CosmicEquipmentComponent, GotUnequippedHandEvent>(OnGotUnheld);

        SubscribeLocalEvent<InfluenceStrideComponent, ComponentInit>(OnStartInfluenceStride);
        SubscribeLocalEvent<InfluenceStrideComponent, ComponentRemove>(OnEndInfluenceStride);
        SubscribeLocalEvent<InfluenceStrideComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<CosmicImposingComponent, ComponentInit>(OnStartImposition);
        SubscribeLocalEvent<CosmicImposingComponent, ComponentRemove>(OnEndImposition);
        SubscribeLocalEvent<CosmicImposingComponent, RefreshMovementSpeedModifiersEvent>(OnImpositionMoveSpeed);
        SubscribeLocalEvent<CosmicEmpoweredSpeedComponent, ComponentInit>(OnStartCosmicEmpowered);
        SubscribeLocalEvent<CosmicEmpoweredSpeedComponent, ComponentRemove>(OnEndCosmicEmpowered);
        SubscribeLocalEvent<CosmicEmpoweredSpeedComponent, RefreshMovementSpeedModifiersEvent>(OnCosmicEmpoweredMove);

        SubscribeLocalEvent<CosmicCultComponent, EncryptionChannelsChangedEvent>(OnTransmitterChannelsChangedCult, after: new[] { typeof(IntrinsicRadioKeySystem) });

        SubscribeLocalEvent<RadioSendAttemptEvent>(OnRadioSendAttempt);
        SubscribeLocalEvent<CosmicJammerComponent, AnchorStateChangedEvent>(OnJammerAnchorStateChange);

        SubscribeLocalEvent<CosmicCultComponent, PolymorphedEvent>(OnCultistPolymorphed);
        SubscribeLocalEvent<SpeechOverrideComponent, GotEquippedEvent>(OnGotSpeechOverrideEquipped);
        SubscribeLocalEvent<SpeechOverrideComponent, GotUnequippedEvent>(OnGotSpeechOverrideUnequipped);

        // Goobstation
        SubscribeLocalEvent<CosmicCultLeadComponent, CosmicCultLeadChangedEvent>(OnCultLeadChanged);

        SubscribeFinale(); //Hook up the cosmic cult finale system
    }

    public void MalignEcho(Entity<CosmicCultComponent> uid)
    {
        if (_cultRule.AssociatedGamerule(uid) is not { } cult)
            return;

        if (cult.Comp.CurrentTier > 1 && !_random.Prob(0.5f))
            Spawn(CosmicEchoVfx, Transform(uid).Coordinates);
    }

    #region Housekeeping

    // Rogue Ascendants use this too, which are generalized MidRoundAntags, so we keep the map around. If you're porting cosmic cult, and do not want rogue ascendants, feel free to move this into selective usage akin to NukeOps base.
    /// <summary>
    /// Creates the Cosmic Void pocket dimension map.
    /// </summary>
    private void OnRoundStart(RoundStartingEvent ev)
    {
        if (_mapLoader.TryLoadMap(_mapPath, out var map, out _, new DeserializationOptions { InitializeMaps = true }))
            _map.SetPaused(map.Value.Comp.MapId, false);
    }

    #endregion

    #region Init Cult
    /// <summary>
    /// Add the starting powers to the cultist.
    /// </summary>
    private void OnStartCultist(Entity<CosmicCultComponent> ent, ref ComponentInit args)
    {
        _eye.RefreshVisibilityMask(ent.Owner);
        _alerts.ShowAlert(ent.Owner, ent.Comp.EntropyAlert);

        if (!HasComp<HumanoidAppearanceComponent>(ent)) return; // Non-humanoids don't get abilities
        foreach (var actionId in ent.Comp.CosmicCultActions)
        {
            var actionEnt = _actions.AddAction(ent, actionId);
            ent.Comp.ActionEntities.Add(actionEnt);
        }
    }

    /// <summary>
    /// Add the Monument summon action to the cult lead.
    /// </summary>
    private void OnStartCultLead(Entity<CosmicCultLeadComponent> uid, ref ComponentInit args)
    {
        if (_cultRule.AssociatedGamerule(uid) is not { } cult)
            return;
        if (!HasComp<HumanoidAppearanceComponent>(uid)) return; // Non-humanoids don't get abilities

        if (!cult.Comp.MonumentPlaced) // There's no monument, grant them an action to place one
            _actions.AddAction(uid, ref uid.Comp.CosmicMonumentPlaceActionEntity, uid.Comp.CosmicMonumentPlaceAction, uid);
        if (cult.Comp.MonumentMoved) return; // If the monument was already moved, don't let them do it again.
        var objectiveQuery = EntityQueryEnumerator<CosmicTierConditionComponent>();
        while (objectiveQuery.MoveNext(out _, out var objectiveComp))
        {
            if (objectiveComp.Tier == 2) // If it's stage 2, give them the move action
                _actions.AddAction(uid, ref uid.Comp.CosmicMonumentMoveActionEntity, uid.Comp.CosmicMonumentMoveAction, uid);
        }
    }


    // Goob method? I think?
    private void OnCultLeadChanged(Entity<CosmicCultLeadComponent> uid, ref CosmicCultLeadChangedEvent args)
    {
        if (_cultRule.AssociatedGamerule(uid) is not { } cult)
            return;

        // If they're the last cultist, or the only one due to admemes, they get special powers.
        if (cult.Comp.TotalCult == 1)
        {
            EnsureComp<LoneCosmicCultLeadComponent>(uid);
            _antag.SendBriefing(uid,
                Loc.GetString("cosmiccult-vote-lone-steward-briefing"),
                Color.FromHex("#4cabb3"),
                new SoundPathSpecifier("/Audio/_DV/CosmicCult/tier_up.ogg"));
        }
        else
        {
            _antag.SendBriefing(uid,
                Loc.GetString("cosmiccult-vote-steward-briefing"),
                Color.FromHex("#4cabb3"),
                new SoundPathSpecifier("/Audio/_DV/CosmicCult/tier_up.ogg"));
        }

        if (cult.Comp.CurrentTier == 2)
        {
            _actions.AddAction(uid,
                ref uid.Comp.CosmicMonumentMoveActionEntity,
                uid.Comp.CosmicMonumentMoveAction,
                uid);
        }

        cult.Comp.CultLeader = uid;
    }
    // Goob end.

    private void OnGetVisMask(Entity<CosmicCultComponent> uid, ref GetVisMaskEvent args)
    {
        args.VisibilityMask |= (int) VisibilityFlags.CosmicCultMonument;
    }
    #endregion

    #region Equipment Pickup
    private void OnGotCosmicItemEquipped(Entity<CosmicEquipmentComponent> ent, ref GotEquippedEvent args)
    {
        if (!EntityIsCultist(args.Equipee))
            _statusEffects.TryAddStatusEffect<CosmicEntropyNonCultistComponent>(args.Equipee, EntropicDegenNonCultist, TimeSpan.FromDays(1), true); // TimeSpan.MaxValue causes a crash here, so we use FromDays(1) instead.
    }

    private void OnGotCosmicItemUnequipped(Entity<CosmicEquipmentComponent> ent, ref GotUnequippedEvent args)
    {
        if (!EntityIsCultist(args.Equipee))
            _statusEffects.TryRemoveStatusEffect(args.Equipee, EntropicDegenNonCultist);
    }
    private void OnGotHeld(Entity<CosmicEquipmentComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!EntityIsCultist(args.User))
        {
            _statusEffects.TryAddStatusEffect<CosmicEntropyNonCultistComponent>(args.User, EntropicDegenNonCultist, TimeSpan.FromDays(1), true);
            _popup.PopupEntity(Loc.GetString("cosmiccult-gear-pickup", ("ITEM", args.Equipped)), args.User, args.User, PopupType.MediumCaution);
        }
    }

    private void OnGotUnheld(Entity<CosmicEquipmentComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!EntityIsCultist(args.User))
            _statusEffects.TryRemoveStatusEffect(args.User, EntropicDegenNonCultist);
    }

    private void OnGotSpeechOverrideEquipped(Entity<SpeechOverrideComponent> ent, ref GotEquippedEvent args)
    {
        if (ent.Comp.OverrideIDs is not { } overrides || !TryComp<VocalComponent>(args.Equipee, out var vocalComp))
            return;
        ent.Comp.StoredIDs = vocalComp.Sounds;
        vocalComp.Sounds = overrides;
        var ev = new SoundsChangedEvent();
        RaiseLocalEvent(args.Equipee, ref ev);
    }

    private void OnGotSpeechOverrideUnequipped(Entity<SpeechOverrideComponent> ent, ref GotUnequippedEvent args)
    {
        if (ent.Comp.StoredIDs is not { } stored || !TryComp<VocalComponent>(args.Equipee, out var vocalComp)) return;
        ent.Comp.StoredIDs = null;
        vocalComp.Sounds = stored;
        var ev = new SoundsChangedEvent();
        RaiseLocalEvent(args.Equipee, ref ev);
    }
    #endregion

    #region Movespeed
    private void OnStartInfluenceStride(Entity<InfluenceStrideComponent> uid, ref ComponentInit args) => // i wish movespeed was easier to work with
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    private void OnEndInfluenceStride(Entity<InfluenceStrideComponent> uid, ref ComponentRemove args) => // that movespeed applies more-or-less correctly
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    private void OnStartImposition(Entity<CosmicImposingComponent> uid, ref ComponentInit args) // these functions just make sure
    {
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    }
    private void OnEndImposition(Entity<CosmicImposingComponent> uid, ref ComponentRemove args) // as various cosmic cult effects get added and removed
    {
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefreshMoveSpeed(EntityUid uid, InfluenceStrideComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(1.15f, 1.15f);
    }
    private void OnImpositionMoveSpeed(EntityUid uid, CosmicImposingComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(0.8f, 0.8f);
    }

    // Goob start
    private void OnStartCosmicEmpowered(Entity<CosmicEmpoweredSpeedComponent> uid, ref ComponentInit args) =>
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    private void OnEndCosmicEmpowered(Entity<CosmicEmpoweredSpeedComponent> uid, ref ComponentRemove args) =>
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    private void OnCosmicEmpoweredMove(EntityUid uid, CosmicEmpoweredSpeedComponent comp, RefreshMovementSpeedModifiersEvent args) =>
        args.ModifySpeed(comp.SpeedBoost, comp.SpeedBoost);
    // Goob end

    #endregion

    #region Edge cases
    /// <summary>
    /// Edge Case to handle IPCs losing astral murmur after panel operations.
    /// </summary>
    private void OnTransmitterChannelsChangedCult(EntityUid uid, CosmicCultComponent component, EncryptionChannelsChangedEvent args)
    {
        if (!TryComp<IntrinsicRadioTransmitterComponent>(uid, out IntrinsicRadioTransmitterComponent? transmitter) || !TryComp<ActiveRadioComponent>(uid, out ActiveRadioComponent? activeRadio))
            return;

        if (transmitter.Channels.Contains(CosmicRadio) && activeRadio.Channels.Contains(CosmicRadio))
            return;

        transmitter.Channels.Add(CosmicRadio);
        activeRadio.Channels.Add(CosmicRadio);


    }

    /// <summary>
    /// When a cultist gets polymorphed, ensure that the resulting entity has all the necessary components. Mostly there for kitsune my behated.
    /// </summary>
    private void OnCultistPolymorphed(Entity<CosmicCultComponent> ent, ref PolymorphedEvent args)
    {
        if (_cultRule.AssociatedGamerule(args.OldEntity) is not { } cult)
            return;
        if (TryComp<CosmicCultComponent>(args.OldEntity, out var oldCultComp))
        {
            EnsureComp<CosmicCultComponent>(args.NewEntity, out var cultComp);
            cultComp.Respiration = oldCultComp.Respiration;
            cultComp.EntropyStored = oldCultComp.EntropyStored;
            cultComp.CosmicEmpowered = oldCultComp.CosmicEmpowered;
            cultComp.StoredDamageContainer = oldCultComp.StoredDamageContainer;
        }
        if (TryComp<CleanseCultComponent>(args.OldEntity, out var oldCleanComp)) // No avoiding deconversion by transforming into a fox
        {
            EnsureComp<CleanseCultComponent>(args.NewEntity, out var cleanComp);
            cleanComp.CleanseTime = oldCleanComp.CleanseTime;
        }
        if (HasComp<CosmicCultLeadComponent>(args.OldEntity))
            EnsureComp<CosmicCultLeadComponent>(args.NewEntity);
        if (HasComp<CosmicStarMarkComponent>(args.OldEntity))
            EnsureComp<CosmicStarMarkComponent>(args.NewEntity);
        if (HasComp<CosmicSubtleMarkComponent>(args.OldEntity))
            EnsureComp<CosmicSubtleMarkComponent>(args.NewEntity);
        if (HasComp<TemperatureImmunityComponent>(args.OldEntity))
            EnsureComp<TemperatureImmunityComponent>(args.NewEntity);
        if (HasComp<PressureImmunityComponent>(args.OldEntity))
            EnsureComp<PressureImmunityComponent>(args.NewEntity);
        EnsureComp<IntrinsicRadioReceiverComponent>(args.NewEntity); // All cultists should have those, so we don't check for them separately
        EnsureComp<IntrinsicRadioTransmitterComponent>(args.NewEntity, out var transmitter);
        EnsureComp<ActiveRadioComponent>(args.NewEntity, out var radio);
        EnsureComp<CosmicCultAssociatedRuleComponent>(args.NewEntity, out var associatedComp);
        EnsureComp<CosmicCenserTargetComponent>(args.NewEntity);
        radio.Channels.Add("CosmicRadio");
        transmitter.Channels.Add("CosmicRadio");
        associatedComp.CultGamerule = cult;

    }

    private void OnCultLeadShutdown(Entity<CosmicCultLeadComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;
        _actions.RemoveAction(ent.Owner, ent.Comp.CosmicMonumentPlaceActionEntity);
        _actions.RemoveAction(ent.Owner, ent.Comp.CosmicMonumentMoveActionEntity);
    }
    #endregion

    #region Cosmic jammer
    private void OnJammerAnchorStateChange(Entity<CosmicJammerComponent> ent, ref AnchorStateChangedEvent args)
    {
        ent.Comp.Active = args.Anchored;
        _ambient.SetAmbience(ent, args.Anchored);
        _lights.SetEnabled(ent, args.Anchored);
    }

    private void OnRadioSendAttempt(ref RadioSendAttemptEvent args)
    {
        if (args.Channel == CosmicRadio) return; // Cult can still communicate within range of a jammer.

        var source = Transform(args.RadioSource).Coordinates;
        var query = EntityQueryEnumerator<CosmicJammerComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var jammer, out var transform))
        {
            if (_transform.InRange(source, transform.Coordinates, jammer.Range) && jammer.Active)
            {
                args.Cancelled = true;
                return;
            }
        }
    }
    #endregion
}
