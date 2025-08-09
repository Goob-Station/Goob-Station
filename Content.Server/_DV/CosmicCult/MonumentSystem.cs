// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._DV.CosmicCult.Components;
using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Goobstation.Shared.Religion; // Goobstation - Shitchap
using Content.Server.Actions;
using Content.Server.Atmos.Components;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Objectives.Components;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Prototypes;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Temperature.Components;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

using Content.Shared._Shitmed.Targeting; // Shitmed Change

namespace Content.Server._DV.CosmicCult;

public sealed class MonumentSystem : SharedMonumentSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly CosmicCorruptingSystem _corrupting = default!;
    [Dependency] private readonly CosmicCultRuleSystem _cosmicRule = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    private static readonly EntProtoId CosmicGod = "MobCosmicGodSpawn";
    private static readonly EntProtoId MonumentCollider = "MonumentCollider";
    private EntityUid? _monumentStorageMap;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MonumentComponent, InteractUsingEvent>(OnInfuseHeldEntropy);
        SubscribeLocalEvent<MonumentComponent, ActivateInWorldEvent>(OnInfuseEntropy);
    }

    public override void Update(float frameTime) // This Update() can fit so much functionality in it
    {
        base.Update(frameTime);

        var finaleQuery = EntityQueryEnumerator<CosmicFinaleComponent, MonumentComponent>(); // Enumerator for The Monument's Finale
        while (finaleQuery.MoveNext(out var uid, out var comp, out var monuComp))
        {
            if (_timing.CurTime >= monuComp.CheckTimer)
            {
                var entities = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, 10);
                entities.RemoveWhere(entity => !HasComp<InfluenceVitalityComponent>(entity));

                foreach (var entity in entities)
                    _damage.TryChangeDamage(entity, monuComp.MonumentHealing * -1, targetPart: TargetBodyPart.All); // Shitmed Change

                monuComp.CheckTimer = _timing.CurTime + monuComp.CheckWait;
            }

            if (comp.SongTimer is { } time
                && _timing.CurTime >= time)
            {
                comp.SongTimer = null;

                if (comp.SelectedSong is { } song)
                    _sound.DispatchStationEventMusic(uid, song, StationEventMusicType.CosmicCult);
            }

            if (comp.CurrentState == FinaleState.ActiveBuffer
                && _timing.CurTime >= comp.BufferTimer) // swap everything over when buffer timer runs out
            {
                comp.CurrentState = FinaleState.ActiveFinale;
                comp.FinaleTimer = _timing.CurTime + comp.FinaleRemainingTime;
                comp.SelectedSong = comp.FinaleMusic;
                _sound.StopStationEventMusic(uid, StationEventMusicType.CosmicCult);
                _appearance.SetData(uid, MonumentVisuals.FinaleReached, 3);
                _chatSystem.DispatchStationAnnouncement(uid,
                    Loc.GetString("cosmiccult-announce-finale-warning"),
                    null,
                    false,
                    null,
                    Color.FromHex("#cae8e8"));
                comp.SongTimer = _timing.CurTime + TimeSpan.FromSeconds(1);
            }
            else if (comp.CurrentState == FinaleState.ActiveFinale && _timing.CurTime >= comp.FinaleTimer) // trigger wincondition on time runout
            {
                var victoryQuery = EntityQueryEnumerator<CosmicVictoryConditionComponent>();

                while (victoryQuery.MoveNext(out _, out var victoryComp))
                    victoryComp.Victory = true;

                _sound.StopStationEventMusic(uid, StationEventMusicType.CosmicCult);
                Spawn(CosmicGod, Transform(uid).Coordinates);
                comp.CurrentState = FinaleState.Victory;
            }
        }

        var monumentQuery = EntityQueryEnumerator<MonumentComponent>();
        while (monumentQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.PhaseOutTimer is { } timer
                && _timing.CurTime >= timer)
            {
                OnMonumentPhaseOut((uid, comp));
                comp.PhaseOutTimer = null;
            }
        }

        var destinationQuery = EntityQueryEnumerator<MonumentMoveDestinationComponent>();
        while (destinationQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.PhaseInTimer is { } timer
                && _timing.CurTime >= timer)
            {
                OnMonumentPhaseIn((uid, comp));
                comp.PhaseInTimer = null;
            }
        }
    }

    private void OnMonumentPhaseOut(Entity<MonumentComponent> ent)
    {
        //todo check if anything gets messed up by doing this to the monument?
        _transform.SetParent(ent, EnsureStorageMapExists());

        if (ent.Comp.CurrentGlyph is not null) //delete the scribed glyph as well
            QueueDel(ent.Comp.CurrentGlyph);

        //close the UI for everyone who has it open
        _ui.CloseUi(ent.Owner, MonumentKey.Key);
    }

    private void OnMonumentPhaseIn(Entity<MonumentMoveDestinationComponent> ent)
    {
        var colliderQuery = EntityQueryEnumerator<MonumentCollisionComponent>();

        while (colliderQuery.MoveNext(out var collider, out _))
            QueueDel(collider);

        if (ent.Comp.Monument is null)
            return;

        var xform = Transform(ent);
        _transform.SetCoordinates(ent.Comp.Monument.Value, xform.Coordinates);
        _transform.AnchorEntity(ent.Comp.Monument.Value); //no idea if this does anything but let's be safe about it
        Spawn(MonumentCollider, xform.Coordinates);

        if (TryComp<CosmicCorruptingComponent>(ent.Comp.Monument.Value, out var cosmicCorruptingComp))
            _corrupting.RecalculateStartingTiles((ent.Comp.Monument.Value, cosmicCorruptingComp));
    }

    private EntityUid EnsureStorageMapExists()
    {
        if (_monumentStorageMap != null
            && Exists(_monumentStorageMap))
            return _monumentStorageMap.Value;

        _monumentStorageMap = _map.CreateMap();
        _map.SetPaused(_monumentStorageMap.Value, true);
        return _monumentStorageMap.Value;
    }

    public void PhaseOutMonument(Entity<MonumentComponent> ent) =>
        ent.Comp.PhaseOutTimer = _timing.CurTime + TimeSpan.FromSeconds(0.45);

    public void UpdateMonumentProgress(Entity<MonumentComponent> ent, Entity<CosmicCultRuleComponent> cult) =>
        ent.Comp.CurrentProgress = ent.Comp.TotalEntropy + cult.Comp.TotalCult * _config.GetCVar(DCCVars.CosmicCultistEntropyValue);

    private void OnInfuseEntropy(Entity<MonumentComponent> uid, ref ActivateInWorldEvent args)
    {
        if (!args.Complex)
            return;

        if (TryComp<CosmicCultComponent>(args.User, out var cultComp)
            && cultComp.EntropyStored > 0)
            args.Handled = AddEntropy(uid, (args.User, cultComp));
    }

    private void OnInfuseHeldEntropy(Entity<MonumentComponent> uid, ref InteractUsingEvent args)
    {
        if (!HasComp<CosmicEntropyMoteComponent>(args.Used)
            || !TryComp<CosmicCultComponent>(args.User, out var cultComp)
            || !uid.Comp.Enabled
            || args.Handled)
        {
            _popup.PopupEntity(Loc.GetString("cosmiccult-entropy-unavailable"), args.User, args.User);
            return;
        }

        args.Handled = AddEntropy(uid, args.Used, (args.User, cultComp));
    }

    /// <summary>
    /// Method for adding the Cultist's internal Entropy to The Monument.
    /// </summary>
    private bool AddEntropy(Entity<MonumentComponent> monument, Entity<CosmicCultComponent> cultist)
    {
        _audio.PlayEntity(_audio.ResolveSound(monument.Comp.InfusionSFX), cultist, monument);
        _popup.PopupEntity(Loc.GetString("cosmiccult-entropy-inserted",
            ("count", cultist.Comp.EntropyStored)),
            cultist,
            cultist);
        monument.Comp.TotalEntropy += cultist.Comp.EntropyStored;
        cultist.Comp.EntropyStored = 0;
        Dirty(cultist, cultist.Comp);
        _cosmicRule.UpdateCultData(monument);
        return true;
    }

    /// <summary>
    /// Method for adding itemized Entropy to The Monument.
    /// </summary>
    private bool AddEntropy(Entity<MonumentComponent> monument, EntityUid entropy, Entity<CosmicCultComponent> cultist)
    {
        var quant = TryComp<StackComponent>(entropy, out var stackComp) ? stackComp.Count : 1;
        monument.Comp.TotalEntropy += quant;
        cultist.Comp.EntropyBudget += quant;
        Dirty(cultist, cultist.Comp);
        _cosmicRule.UpdateCultData(monument);
        _popup.PopupEntity(Loc.GetString("cosmiccult-entropy-inserted", ("count", quant)), cultist, cultist);
        _audio.PlayEntity(_audio.ResolveSound(monument.Comp.InfusionSFX), cultist, monument);
        QueueDel(entropy);
        return true;
    }

    public void UpdateMonumentAppearance(Entity<MonumentComponent> ent, bool tierUp) // this is kinda awful, but it works, and i've seen worse. improve it at thine leisure
    {
        if (_cosmicRule.AssociatedGamerule(ent) is not { } cult
            || !TryComp<CosmicFinaleComponent>(ent, out var finaleComp))
            return;

        _appearance.SetData(ent, MonumentVisuals.Monument, cult.Comp.CurrentTier);

        switch (cult.Comp.CurrentTier)
        {
            case 3:
                _appearance.SetData(ent, MonumentVisuals.Tier3, true);
                break;
            case 2:
                _appearance.SetData(ent, MonumentVisuals.Tier3, false);
                break;
        }

        if (tierUp)
        {
            var transformComp = EnsureComp<MonumentTransformingComponent>(ent);
            transformComp.EndTime = _timing.CurTime + ent.Comp.TransformTime;
            _appearance.SetData(ent, MonumentVisuals.Transforming, true);
        }

        if (finaleComp.CurrentState != FinaleState.Unavailable)
            _appearance.SetData(ent, MonumentVisuals.FinaleReached, true);
    }

    //note - these ar the thresholds for moving to the next tier
    //so t1 -> 2 needs 20% of the crew
    //t2 -> 3 needs 40%
    //and t3 -> finale needs an extra 20 entropy
    public void UpdateMonumentReqsForTier(Entity<MonumentComponent> monument, int tier)
    {
        if (_cosmicRule.AssociatedGamerule(monument) is not { } cult)
            return;

        var numberOfCrewForTier3 = Math.Round((double) cult.Comp.TotalCrew / 100 * _config.GetCVar(DCCVars.CosmicCultTargetConversionPercent)); // 40% of current pop
        var difficultyMultiplier = _config.GetCVar(DCCVars.CosmicCultistDifficultyMultiplier);
        switch (tier)
        {
            case 1:
                monument.Comp.ProgressOffset = 0;
                monument.Comp.TargetProgress = (int) (difficultyMultiplier * numberOfCrewForTier3 / 2 * _config.GetCVar(DCCVars.CosmicCultistEntropyValue));
                break;
            case 2:
                monument.Comp.ProgressOffset = (int) (difficultyMultiplier * numberOfCrewForTier3 / 2 * _config.GetCVar(DCCVars.CosmicCultistEntropyValue)); //reset the progress offset
                monument.Comp.TargetProgress = (int) (difficultyMultiplier * numberOfCrewForTier3 * _config.GetCVar(DCCVars.CosmicCultistEntropyValue));
                break;
            case 3:
                monument.Comp.ProgressOffset = (int) (difficultyMultiplier * numberOfCrewForTier3 * _config.GetCVar(DCCVars.CosmicCultistEntropyValue));
                monument.Comp.TargetProgress = (int) (difficultyMultiplier * numberOfCrewForTier3 * _config.GetCVar(DCCVars.CosmicCultistEntropyValue)); //removed offset; replaced with timer
                break;
        }
    }

    public void SetCanTierUp(Entity<MonumentComponent> ent, bool canTierUp) =>
        ent.Comp.CanTierUp = canTierUp;

    public void SetTargetProgess(Entity<MonumentComponent> ent, int targetProgress) =>
        ent.Comp.TargetProgress = targetProgress;

    public void Disable(Entity<MonumentComponent> ent) =>
        ent.Comp.Enabled = false;

    public void Enable(Entity<MonumentComponent> ent) =>
        ent.Comp.Enabled = true;

    public void MonumentTier1(Entity<MonumentComponent> uid)
    {
        if (_cosmicRule.AssociatedGamerule(uid) is not { } cult)
            return;

        UpdateMonumentAppearance(uid, false);

        //this is probably unnecessary but I have no idea where they get added to the list atm - ruddygreat
        foreach (var glyphProto in _protoMan
            .EnumeratePrototypes<GlyphPrototype>()
            .Where(proto => proto.Tier == 1))
            uid.Comp.UnlockedGlyphs.Add(glyphProto.ID);

        //basically completely unnecessary, but putting this here for sanity & futureproofing - ruddygreat
        var query = EntityQueryEnumerator<CosmicCultComponent>();
        while (query.MoveNext(out var cultist, out var cultComp))
        {
            foreach (var influenceProto in _protoMan
                .EnumeratePrototypes<InfluencePrototype>()
                .Where(influenceProto => influenceProto.Tier == 1))
                cultComp.UnlockedInfluences.Add(influenceProto.ID);

            Dirty(cultist, cultComp);
        }

        var objectiveQuery = EntityQueryEnumerator<CosmicTierConditionComponent>();

        while (objectiveQuery.MoveNext(out _, out var objectiveComp))
            objectiveComp.Tier = 1;
    }

    public void MonumentTier2(Entity<MonumentComponent> uid)
    {
        if (_cosmicRule.AssociatedGamerule(uid) is not { } cult)
            return;

        UpdateMonumentAppearance(uid, true);

        foreach (var glyphProto in _protoMan
            .EnumeratePrototypes<GlyphPrototype>()
            .Where(proto => proto.Tier == 2))
            uid.Comp.UnlockedGlyphs.Add(glyphProto.ID);

        var objectiveQuery = EntityQueryEnumerator<CosmicTierConditionComponent>();

        while (objectiveQuery.MoveNext(out _, out var objectiveComp))
            objectiveComp.Tier = 2;

        var query = EntityQueryEnumerator<CosmicCultComponent>();
        while (query.MoveNext(out var cultist, out var cultComp))
        {
            foreach (var influenceProto in _protoMan.EnumeratePrototypes<InfluencePrototype>().Where(influenceProto => influenceProto.Tier == 2))
                cultComp.UnlockedInfluences.Add(influenceProto.ID);

            cultComp.EntropyBudget += cult.Comp.TotalCrew / 100 * 10; // pity system. 10% of the playercount worth of entropy on tier up

            Dirty(cultist, cultComp);
        }

        //add the move action
        var leaderQuery = EntityQueryEnumerator<CosmicCultLeadComponent>();

        while (leaderQuery.MoveNext(out var leader, out var leaderComp))
            _actions.AddAction(leader, ref leaderComp.CosmicMonumentMoveActionEntity, leaderComp.CosmicMonumentMoveAction, leader);

        Dirty(uid);
    }

    public void MonumentTier3(Entity<MonumentComponent> uid)
    {
        if (_cosmicRule.AssociatedGamerule(uid) is not { } cult)
            return;

        foreach (var glyphProto in _protoMan
            .EnumeratePrototypes<GlyphPrototype>()
            .Where(proto => proto.Tier == 3))
            uid.Comp.UnlockedGlyphs.Add(glyphProto.ID);

        UpdateMonumentAppearance(uid, true);

        var objectiveQuery = EntityQueryEnumerator<CosmicTierConditionComponent>();
        while (objectiveQuery.MoveNext(out var _, out var objectiveComp))
            objectiveComp.Tier = 3;

        var query = EntityQueryEnumerator<CosmicCultComponent>();
        while (query.MoveNext(out var cultist, out var cultComp))
        {
            EnsureComp<PressureImmunityComponent>(cultist);
            EnsureComp<TemperatureImmunityComponent>(cultist);

            // Goobstation Change - Shitchap
            if (!HasComp<WeakToHolyComponent>(cultist))
                EnsureComp<WeakToHolyComponent>(cultist).AlwaysTakeHoly = true;
            else
                cultComp.WasWeakToHoly = true;

            foreach (var influenceProto in _protoMan.EnumeratePrototypes<InfluencePrototype>().Where(influenceProto => influenceProto.Tier == 3))
                cultComp.UnlockedInfluences.Add(influenceProto.ID);

            cultComp.Respiration = false;
            cultComp.EntropyBudget += cult.Comp.TotalCrew / 100 * 10; //pity system. 10% of the playercount worth of entropy on tier up
            Dirty(cultist, cultComp);
        }

        //remove the move action
        var leaderQuery = EntityQueryEnumerator<CosmicCultLeadComponent>();
        while (leaderQuery.MoveNext(out var leader, out var leaderComp))
            _actions.RemoveAction(leader, leaderComp.CosmicMonumentMoveActionEntity);
        Dirty(uid);
    }

    public void ReadyFinale(Entity<MonumentComponent> uid, CosmicFinaleComponent finaleComp)
    {
        if (TryComp<CosmicCorruptingComponent>(uid, out var comp))
            _corrupting.Enable((uid, comp));

        if (TryComp<ActivatableUIComponent>(uid, out var uiComp))
        {
            if (TryComp<UserInterfaceComponent>(uid, out var uiComp2)) //close the UI for everyone who has it open
                _ui.CloseUi((uid.Owner, uiComp2), MonumentKey.Key);

            uiComp.Key = null; //kazne called this the laziest way to disable a UI ever
        }

        finaleComp.CurrentState = FinaleState.ReadyBuffer;
        uid.Comp.Enabled = false;
        uid.Comp.TargetProgress = uid.Comp.CurrentProgress;

        _popup.PopupCoordinates(Loc.GetString("cosmiccult-finale-ready"), Transform(uid).Coordinates, PopupType.Large);
    }
}
