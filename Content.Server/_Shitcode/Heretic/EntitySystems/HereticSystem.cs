// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GMWQ <garethquaile@gmail.com>
// SPDX-FileCopyrightText: 2025 Gareth Quaile <garethquaile@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gonz0 <105350621+doktor-gonz0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gonz0 < 105350621+doktor-gonz0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Server.Store.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Eye;
using Content.Shared.Heretic;
using Content.Shared.Mind;
using Content.Shared.Store.Components;
using Content.Shared.Heretic.Prototypes;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio;
using Content.Server.Heretic.Components;
using Content.Server.Antag;
using Robust.Shared.Random;
using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Server._Goobstation.Objectives.Components;
using Content.Server.Actions;
using Content.Server.Chat.Managers;
using Content.Server.Objectives;
using Content.Shared.Humanoid;
using Robust.Server.Player;
using Content.Server.Revolutionary.Components;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Preferences;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles.Jobs;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Heretic.EntitySystems;

public sealed class HereticSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly HereticKnowledgeSystem _knowledge = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;

    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IChatManager _chatMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private float _timer;
    private const float PassivePointCooldown = 20f * 60f;
    private bool _ascensionRequiresObjectives;

    private const int HereticVisFlags = (int) (VisibilityFlags.EldritchInfluence | VisibilityFlags.EldritchInfluenceSpent);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<HereticComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HereticComponent, GetVisMaskEvent>(OnGetVisMask);
        SubscribeLocalEvent<HereticComponent, EventHereticUpdateTargets>(OnUpdateTargets);
        SubscribeLocalEvent<HereticComponent, EventHereticRerollTargets>(OnRerollTargets);
        SubscribeLocalEvent<HereticComponent, EventHereticAscension>(OnAscension);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);

        Subs.CVar(_cfg, GoobCVars.AscensionRequiresObjectives, value => _ascensionRequiresObjectives = value, true);
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        _timer = 0f;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _timer += frameTime;

        if (_timer < PassivePointCooldown)
            return;

        _timer = 0f;

        foreach (var heretic in EntityQuery<HereticComponent>())
        {
            // passive point gain every 20 minutes
            UpdateKnowledge(heretic.Owner, heretic, 1f);
        }
    }

    public bool ObjectivesAllowAscension(Entity<HereticComponent> ent)
    {
        if (!_ascensionRequiresObjectives)
            return true;

        if (!_mind.TryGetMind(ent, out var mindId, out var mind))
            return false;

        foreach (var objId in ent.Comp.AllObjectives)
        {
            if (_mind.TryFindObjective((mindId, mind), objId, out var obj) &&
                !_objectives.IsCompleted(obj.Value, (mindId, mind)))
                return false;
        }

        return true;
    }

    public void UpdateKnowledge(EntityUid uid,
        HereticComponent comp,
        float amount,
        StoreComponent? store = null,
        bool showText = true,
        bool playSound = true)
    {
        if (Resolve(uid, ref store, false))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "KnowledgePoint", amount } }, uid, store);
            _store.UpdateUserInterface(uid, uid, store);
        }

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        if (_mind.TryGetObjectiveComp<HereticKnowledgeConditionComponent>(mindId, out var objective, mind))
            objective.Researched += amount;

        if (!showText && !playSound)
            return;

        if (!_playerMan.TryGetSessionById(mind.UserId, out var session))
            return;

        if (playSound)
            _audio.PlayGlobal(comp.InfluenceGainSound, session);

        if (!showText)
            return;

        var baseMessage = comp.InfluenceGainBaseMessage;
        var message = Loc.GetString(_rand.Pick(comp.InfluenceGainMessages));
        var size = comp.InfluenceGainTextFontSize;
        var loc = Loc.GetString(baseMessage, ("size", size), ("text", message));
        SharedChatSystem.UpdateFontSize(size, ref message, ref loc);
        _chatMan.ChatMessageToOne(ChatChannel.Server, message, loc, default, false, session.Channel, canCoalesce: false);
    }

    public HashSet<ProtoId<TagPrototype>>? TryGetRequiredKnowledgeTags(Entity<HereticComponent> ent)
    {
        if (ent.Comp.KnowledgeRequiredTags.Count > 0 || GenerateRequiredKnowledgeTags(ent))
            return ent.Comp.KnowledgeRequiredTags;

        return null;
    }

    public bool GenerateRequiredKnowledgeTags(Entity<HereticComponent> ent)
    {
        ent.Comp.KnowledgeRequiredTags.Clear();
        var dataset = _proto.Index(ent.Comp.KnowledgeDataset);
        for (var i = 0; i < 4; i++)
        {
            ent.Comp.KnowledgeRequiredTags.Add(_rand.Pick(dataset));
        }

        return ent.Comp.KnowledgeRequiredTags.Count > 0;
    }

    private void OnCompInit(Entity<HereticComponent> ent, ref ComponentInit args)
    {
        // add influence layer
        if (TryComp<EyeComponent>(ent,
                out var eye)) // As a result, I'm afraid its complete shitcode however it's working shitcode.
            _eye.SetVisibilityMask(ent, eye.VisibilityMask | HereticVisFlags, eye);

        foreach (var knowledge in ent.Comp.BaseKnowledge)
            _knowledge.AddKnowledge(ent, ent.Comp, knowledge);

        GenerateRequiredKnowledgeTags(ent);
        RaiseLocalEvent(ent, new EventHereticRerollTargets());
    }

    private void OnShutdown(Entity<HereticComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<EyeComponent>(ent, out var eye))
            _eye.SetVisibilityMask(ent, eye.VisibilityMask & ~HereticVisFlags, eye);

        foreach (var action in ent.Comp.ProvidedActions)
            _actions.RemoveAction(action);
    }

    private void OnGetVisMask(Entity<HereticComponent> uid, ref GetVisMaskEvent args)
    {
        args.VisibilityMask |= HereticVisFlags;
    }

    #region Internal events (target reroll, ascension, etc.)

    private void OnUpdateTargets(Entity<HereticComponent> ent, ref EventHereticUpdateTargets args)
    {
        ent.Comp.SacrificeTargets = ent.Comp.SacrificeTargets
            .Where(target => TryGetEntity(target.Entity, out var tent) && Exists(tent) &&
                             !EntityManager.IsQueuedForDeletion(tent.Value))
            .ToList();
        Dirty(ent); // update client
    }

    private void OnRerollTargets(Entity<HereticComponent> ent, ref EventHereticRerollTargets args)
    {
        // welcome to my linq smorgasbord of doom
        // have fun figuring that out

        var targets = _antag.GetAliveConnectedPlayers(_playerMan.Sessions)
            .Where(IsSessionValid)
            .Select(x => x.AttachedEntity!.Value)
            .ToList();

        var pickedTargets = new List<EntityUid>();

        var predicates = new List<Func<EntityUid, bool>>();

        // pick one command staff
        predicates.Add(HasComp<CommandStaffComponent>);
        // pick one security staff
        predicates.Add(HasComp<SecurityStaffComponent>);

        // add more predicates here

        foreach (var predicate in predicates)
        {
            var list = targets.Where(predicate).ToList();

            if (list.Count == 0)
                continue;

            // pick and take
            var picked = _rand.Pick(list);
            targets.Remove(picked);
            pickedTargets.Add(picked);
        }

        // add whatever more until satisfied
        for (var i = 0; i <= ent.Comp.MaxTargets - pickedTargets.Count; i++)
        {
            if (targets.Count > 0)
                pickedTargets.Add(_rand.PickAndTake(targets));
        }

        // leave only unique entityuids
        pickedTargets = pickedTargets.Distinct().ToList();

        ent.Comp.SacrificeTargets = pickedTargets.Select(GetData).OfType<SacrificeTargetData>().ToList();
        Dirty(ent); // update client

        return;

        bool IsSessionValid(ICommonSession session)
        {
            if (!HasComp<HumanoidAppearanceComponent>(session.AttachedEntity))
                return false;

            if (HasComp<GhoulComponent>(session.AttachedEntity.Value) ||
                HasComp<HereticComponent>(session.AttachedEntity.Value))
                return false;

            return _mind.TryGetMind(session.AttachedEntity.Value, out var mind, out _) &&
                   _job.MindTryGetJobId(mind, out _);
        }
    }

    private SacrificeTargetData? GetData(EntityUid uid)
    {
        if (!TryComp(uid, out HumanoidAppearanceComponent? humanoid))
            return null;

        if (!_mind.TryGetMind(uid, out var mind, out _) || !_job.MindTryGetJobId(mind, out var jobId) || jobId == null)
            return null;

        var hair = (HairStyles.DefaultHairStyle, humanoid.CachedHairColor ?? Color.Black);
        if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.Hair, out var hairMarkings) && hairMarkings.Count > 0)
        {
            var hairMarking = hairMarkings[0];
            hair = (hairMarking.MarkingId, hairMarking.MarkingColors.FirstOrNull() ?? Color.Black);
        }

        var facialHair = (HairStyles.DefaultFacialHairStyle, humanoid.CachedFacialHairColor ?? Color.Black);
        if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.FacialHair, out var facialHairMarkings) &&
            facialHairMarkings.Count > 0)
        {
            var facialHairMarking = facialHairMarkings[0];
            facialHair = (facialHairMarking.MarkingId, facialHairMarking.MarkingColors.FirstOrNull() ?? Color.Black);
        }

        var appearance = new HumanoidCharacterAppearance(hair.Item1,
            hair.Item2,
            facialHair.Item1,
            facialHair.Item2,
            humanoid.EyeColor,
            humanoid.SkinColor,
            humanoid.MarkingSet.GetForwardEnumerator().ToList());

        var profile = new HumanoidCharacterProfile().WithGender(humanoid.Gender)
            .WithSex(humanoid.Sex)
            .WithSpecies(humanoid.Species)
            .WithName(MetaData(uid).EntityName)
            .WithAge(humanoid.Age)
            .WithCharacterAppearance(appearance);

        var netEntity = GetNetEntity(uid);

        return new SacrificeTargetData { Entity = netEntity, Profile = profile, Job = jobId.Value };
    }

    // notify the crew of how good the person is and play the cool sound :godo:
    private void OnAscension(Entity<HereticComponent> ent, ref EventHereticAscension args)
    {
        // you've already ascended, man.
        if (ent.Comp.Ascended || !ent.Comp.CanAscend)
            return;

        ent.Comp.Ascended = true;
        ent.Comp.KnownRituals.Remove("FeastOfOwls");
        ent.Comp.ChosenRitual = null;
        Dirty(ent);

        // how???
        if (ent.Comp.CurrentPath == null)
            return;

        foreach (var (action, _) in _actions.GetActions(ent))
        {
            if (TryComp(action, out ChangeUseDelayOnAscensionComponent? changeUseDelay) &&
                (changeUseDelay.RequiredPath == null || changeUseDelay.RequiredPath == ent.Comp.CurrentPath))
                _actions.SetUseDelay(action, changeUseDelay.NewUseDelay);
        }

        var pathLoc = ent.Comp.CurrentPath.ToLower();
        var ascendSound =
            new SoundPathSpecifier($"/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/ascend_{pathLoc}.ogg");
        _chat.DispatchGlobalAnnouncement(Loc.GetString($"heretic-ascension-{pathLoc}"),
            Name(ent),
            true,
            ascendSound,
            Color.Pink);
    }

    #endregion
}
