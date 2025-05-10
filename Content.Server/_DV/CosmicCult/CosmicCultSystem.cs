// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Server._DV.CosmicCult.Components;
using Content.Goobstation.Shared.Religion; // Goobstation - Shitchap
using Content.Server.Actions;
using Content.Server.AlertLevel;
using Content.Server.Audio;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Events;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Eye;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
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

namespace Content.Server._DV.CosmicCult;

public sealed partial class CosmicCultSystem : SharedCosmicCultSystem
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
    private readonly ResPath _mapPath = new("Maps/_DV/Nonstations/cosmicvoid.yml");
    private static readonly EntProtoId CosmicEchoVfx = "CosmicEchoVfx";
    private static readonly ProtoId<StatusEffectPrototype> EntropicDegen = "EntropicDegen";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);

        SubscribeLocalEvent<CosmicCultComponent, ComponentInit>(OnStartCultist);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentInit>(OnStartCultLead);
        SubscribeLocalEvent<CosmicCultLeadComponent, CosmicCultLeadChangedEvent>(OnCultLeadChanged);
        SubscribeLocalEvent<CosmicCultComponent, GetVisMaskEvent>(OnGetVisMask);

        SubscribeLocalEvent<CosmicEquipmentComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<CosmicEquipmentComponent, GotUnequippedEvent>(OnGotUnequipped);
        SubscribeLocalEvent<CosmicEquipmentComponent, GotEquippedHandEvent>(OnGotHeld);
        SubscribeLocalEvent<CosmicEquipmentComponent, GotUnequippedHandEvent>(OnGotUnheld);

        SubscribeLocalEvent<InfluenceStrideComponent, ComponentInit>(OnStartInfluenceStride);
        SubscribeLocalEvent<InfluenceStrideComponent, ComponentRemove>(OnEndInfluenceStride);
        SubscribeLocalEvent<InfluenceStrideComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<CosmicImposingComponent, ComponentInit>(OnStartImposition);
        SubscribeLocalEvent<CosmicImposingComponent, ComponentRemove>(OnEndImposition);
        SubscribeLocalEvent<CosmicImposingComponent, RefreshMovementSpeedModifiersEvent>(OnImpositionMoveSpeed);

        SubscribeLocalEvent<CosmicCultExamineComponent, ExaminedEvent>(OnCosmicCultExamined);

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

    private void OnCosmicCultExamined(Entity<CosmicCultExamineComponent> ent, ref ExaminedEvent args) =>
        args.PushMarkup(Loc.GetString(EntitySeesCult(args.Examiner)
            ? ent.Comp.CultistText
            : ent.Comp.OthersText));

    #endregion

    #region Init Cult
    /// <summary>
    /// Add the starting powers to the cultist.
    /// </summary>
    private void OnStartCultist(Entity<CosmicCultComponent> uid, ref ComponentInit args)
    {
        foreach (var actionId in uid.Comp.CosmicCultActions)
        {
            var actionEnt = _actions.AddAction(uid, actionId);
            uid.Comp.ActionEntities.Add(actionEnt);
        }

        _alerts.ShowAlert(uid, uid.Comp.EntropyAlert);

        if (TryComp(uid, out EyeComponent? eyeComp))
            _eye.SetVisibilityMask(uid, eyeComp.VisibilityMask | (int) VisibilityFlags.CosmicCultMonument);
    }

    /// <summary>
    /// Add the Monument summon action to the cult lead.
    /// </summary>
    private void OnStartCultLead(Entity<CosmicCultLeadComponent> uid, ref ComponentInit args)
    {
        _actions.AddAction(uid, ref uid.Comp.CosmicMonumentPlaceActionEntity, uid.Comp.CosmicMonumentPlaceAction, uid);
    }

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

    private void OnGetVisMask(Entity<CosmicCultComponent> uid, ref GetVisMaskEvent args)
    {
        args.VisibilityMask |= (int) VisibilityFlags.CosmicCultMonument;
    }

    /// <summary>
    /// Called by Cosmic Siphon. Increments the Cult's global objective tracker.
    /// </summary>
    #endregion

    #region Equipment Pickup
    private void OnGotEquipped(Entity<CosmicEquipmentComponent> ent, ref GotEquippedEvent args)
    {
        if (!EntityIsCultist(args.Equipee))
            _statusEffects.TryAddStatusEffect<CosmicEntropyDebuffComponent>(args.Equipee, EntropicDegen, TimeSpan.FromDays(1), true); // TimeSpan.MaxValue causes a crash here, so we use FromDays(1) instead.
    }

    private void OnGotUnequipped(Entity<CosmicEquipmentComponent> ent, ref GotUnequippedEvent args)
    {
        if (!EntityIsCultist(args.Equipee))
            _statusEffects.TryRemoveStatusEffect(args.Equipee, EntropicDegen);
    }
    private void OnGotHeld(Entity<CosmicEquipmentComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!EntityIsCultist(args.User))
        {
            _statusEffects.TryAddStatusEffect<CosmicEntropyDebuffComponent>(args.User, EntropicDegen, TimeSpan.FromDays(1), true);
            _popup.PopupEntity(Loc.GetString("cosmiccult-gear-pickup", ("ITEM", args.Equipped)), args.User, args.User, PopupType.MediumCaution);
        }
    }

    private void OnGotUnheld(Entity<CosmicEquipmentComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!EntityIsCultist(args.User))
            _statusEffects.TryRemoveStatusEffect(args.User, EntropicDegen);
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
        EnsureComp<CosmicCultExamineComponent>(uid).CultistText = "cosmic-examine-text-malignecho";
    }
    private void OnEndImposition(Entity<CosmicImposingComponent> uid, ref ComponentRemove args) // as various cosmic cult effects get added and removed
    {
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
        RemComp<CosmicCultExamineComponent>(uid);
    }

    private void OnRefreshMoveSpeed(EntityUid uid, InfluenceStrideComponent comp, RefreshMovementSpeedModifiersEvent args) =>
        args.ModifySpeed(1.1f, 1.1f);
    private void OnImpositionMoveSpeed(EntityUid uid, CosmicImposingComponent comp, RefreshMovementSpeedModifiersEvent args) =>
        args.ModifySpeed(0.65f, 0.65f);

    #endregion

}
