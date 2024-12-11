using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.Body.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Hands.Systems;
using Content.Server.Language;
using Content.Server.Mind;
using Content.Server.NPC.Systems;
using Content.Server.Pinpointer;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.WhiteDream.BloodCult.Items.BloodSpear;
using Content.Server.WhiteDream.BloodCult.Objectives;
using Content.Server.WhiteDream.BloodCult.RendingRunePlacement;
using Content.Server.WhiteDream.BloodCult.Spells;
using Content.Shared.Cloning;
using Content.Shared.Cuffs.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mood;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.WhiteDream.BloodCult.Components;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Items;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult.Gamerule;

public sealed class BloodCultRuleSystem : GameRuleSystem<BloodCultRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;
    [Dependency] private readonly BloodSpearSystem _bloodSpear = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly LanguageSystem _language = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);

        SubscribeLocalEvent<BloodCultNarsieSummoned>(OnNarsieSummon);

        SubscribeLocalEvent<BloodCultistComponent, ComponentInit>(OnCultistComponentInit);
        SubscribeLocalEvent<BloodCultistComponent, ComponentRemove>(OnCultistComponentRemoved);
        SubscribeLocalEvent<BloodCultistComponent, MobStateChangedEvent>(OnCultistsStateChanged);
        SubscribeLocalEvent<BloodCultistComponent, CloningEvent>(OnClone);

        SubscribeLocalEvent<BloodCultistRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    protected override void Started(
        EntityUid uid,
        BloodCultRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args
    )
    {
        base.Started(uid, component, gameRule, args);

        GetRandomRunePlacements(component);
    }

    protected override void AppendRoundEndText(
        EntityUid uid,
        BloodCultRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args
    )
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);
        var winText = Loc.GetString($"blood-cult-condition-{component.WinCondition.ToString().ToLower()}");
        args.AddLine(winText);

        args.AddLine(Loc.GetString("blood-cultists-list-start"));

        var sessionData = _antagSelection.GetAntagIdentifiers(uid);
        foreach (var (_, data, name) in sessionData)
        {
            var lising = Loc.GetString("blood-cultists-list-name", ("name", name), ("user", data.UserName));
            args.AddLine(lising);
        }
    }

    #region EventHandlers

    private void AfterEntitySelected(Entity<BloodCultRuleComponent> ent, ref AfterAntagEntitySelectedEvent args) =>
        MakeCultist(args.EntityUid, ent);

    private void OnNarsieSummon(BloodCultNarsieSummoned ev)
    {
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var cult, out _))
        {
            cult.WinCondition = CultWinCondition.Win;
            _roundEnd.EndRound();

            foreach (var ent in cult.Cultists)
            {
                if (Deleted(ent.Owner) || !TryComp(ent.Owner, out MindContainerComponent? mindContainer) ||
                    !mindContainer.Mind.HasValue)
                    continue;

                var harvester = Spawn(cult.HarvesterPrototype, Transform(ent.Owner).Coordinates);
                _mind.TransferTo(mindContainer.Mind.Value, harvester);
                _body.GibBody(ent);
            }

            return;
        }
    }

    private void OnCultistComponentInit(Entity<BloodCultistComponent> cultist, ref ComponentInit args)
    {
        RaiseLocalEvent(cultist, new MoodEffectEvent("CultFocused"));
        _language.AddLanguage(cultist, cultist.Comp.CultLanguageId);

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            cult.Cultists.Add(cultist);
            UpdateCultStage(cult);
        }
    }

    private void OnCultistComponentRemoved(Entity<BloodCultistComponent> cultist, ref ComponentRemove args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
            cult.Cultists.Remove(cultist);

        CheckWinCondition();

        if (TerminatingOrDeleted(cultist.Owner))
            return;

        RemoveAllCultItems(cultist);
        RemoveCultistAppearance(cultist);
        RemoveObjectiveAndRole(cultist.Owner);
        RaiseLocalEvent(cultist.Owner, new MoodRemoveEffectEvent("CultFocused"));
        _language.RemoveLanguage(cultist.Owner, cultist.Comp.CultLanguageId);

        if (!TryComp(cultist, out BloodCultSpellsHolderComponent? powersHolder))
            return;

        foreach (var power in powersHolder.SelectedSpells)
            _actions.RemoveAction(cultist.Owner, power);
    }

    private void OnCultistsStateChanged(Entity<BloodCultistComponent> cultist, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            CheckWinCondition();
    }

    private void OnClone(Entity<BloodCultistComponent> cultist, ref CloningEvent args) =>
        RemoveObjectiveAndRole(cultist);

    private void OnGetBriefing(Entity<BloodCultistRoleComponent> cultist, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("blood-cult-role-briefing-short"));
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var rule, out _))
        {
            if (!rule.EmergencyMarkersMode)
                continue;

            args.Append(
                Loc.GetString("blood-cult-role-briefing-emergency-rending", ("amount", rule.EmergencyMarkersCount)));
            return;
        }

        var query = EntityQueryEnumerator<RendingRunePlacementMarkerComponent>();
        while (query.MoveNext(out var uid, out var marker))
        {
            if (!marker.IsActive)
                continue;

            var navMapLocation = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString(uid));
            var coordinates = Transform(uid).Coordinates;
            var msg = Loc.GetString(
                "blood-cult-role-briefing-rending-locations",
                ("location", navMapLocation),
                ("coordinates", coordinates.Position));
            args.Append(Loc.GetString(msg));
        }
    }

    #endregion

    public void Convert(EntityUid target)
    {
        if (!TryComp(target, out ActorComponent? actor))
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var ruleUid, out _, out _, out _))
        {
            if (!TryComp(ruleUid, out AntagSelectionComponent? antagSelection))
                continue;

            var antagSelectionEnt = (ruleUid, antagSelection);
            if (!_antagSelection.TryGetNextAvailableDefinition(antagSelectionEnt, out var def))
                def = antagSelection.Definitions.Last();

            _antagSelection.MakeAntag(antagSelectionEnt, actor.PlayerSession, def.Value);
        }
    }

    public bool IsObjectiveFinished() =>
        !TryGetTarget(out var target) || !HasComp<MobStateComponent>(target) || _mobState.IsDead(target.Value);

    public bool TryGetTarget([NotNullWhen(true)] out EntityUid? target)
    {
        target = GetTarget();
        return target is not null;
    }

    public EntityUid? GetTarget()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var bloodCultRule, out _))
            if (bloodCultRule.OfferingTarget.HasValue)
                return bloodCultRule.OfferingTarget.Value;

        return null;
    }

    public bool IsTarget(EntityUid entityUid)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var rule, out _))
            return entityUid == rule.OfferingTarget;

        return false;
    }

    public int GetTotalCultists()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var rule, out _))
            return rule.Cultists.Count + rule.Constructs.Count;

        return 0;
    }

    public void RemoveObjectiveAndRole(EntityUid uid)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        var objectives = mind.Objectives.FindAll(HasComp<KillTargetCultComponent>);
        foreach (var obj in objectives)
            _mind.TryRemoveObjective(mindId, mind, mind.Objectives.IndexOf(obj));

        if (_role.MindHasRole<BloodCultistRoleComponent>(mindId))
            _role.MindRemoveRole<BloodCultistRoleComponent>(mindId);
    }

    public bool CanDrawRendingRune(EntityUid user)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out var rule, out _))
            if (rule is { EmergencyMarkersMode: true, EmergencyMarkersCount: > 0 })
            {
                rule.EmergencyMarkersCount--;
                return true;
            }

        var query = EntityQueryEnumerator<RendingRunePlacementMarkerComponent>();
        while (query.MoveNext(out var uid, out var marker))
        {
            if (!marker.IsActive)
                continue;

            var userLocation = Transform(user).Coordinates;
            var placementCoordinates = Transform(uid).Coordinates;
            if (_transform.InRange(placementCoordinates, userLocation, marker.DrawingRange))
                return true;
        }

        return false;
    }

    public void SetRandomCultTarget(BloodCultRuleComponent rule)
    {
        var querry = EntityManager
            .EntityQueryEnumerator<MindContainerComponent, HumanoidAppearanceComponent, ActorComponent>();

        var potentialTargets = new List<EntityUid>();

        // Cultists not being excluded from target selection is fully intended.
        while (querry.MoveNext(out var uid, out _, out _, out _))
            potentialTargets.Add(uid);

        rule.OfferingTarget = potentialTargets.Count > 0 ? _random.Pick(potentialTargets) : null;
    }

    public bool TryConsumeNearestMarker(EntityUid user)
    {
        var ruleQuery = QueryActiveRules();
        while (ruleQuery.MoveNext(out _, out var rule, out _))
            if (rule is { EmergencyMarkersMode: true, EmergencyMarkersCount: > 0 })
            {
                rule.EmergencyMarkersCount--;
                return true;
            }

        var userLocation = Transform(user).Coordinates;
        var query = EntityQueryEnumerator<RendingRunePlacementMarkerComponent>();
        while (query.MoveNext(out var markerUid, out var marker))
        {
            if (!marker.IsActive)
                continue;

            var placementCoordinates = Transform(markerUid).Coordinates;
            if (!_transform.InRange(placementCoordinates, userLocation, marker.DrawingRange))
                continue;

            marker.IsActive = false;
            break;
        }

        return false;
    }

    private void CheckWinCondition()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            var aliveCultists = cult.Cultists.Count(cultist => !_mobState.IsDead(cultist));
            if (aliveCultists != 0)
                return;

            cult.WinCondition = CultWinCondition.Failure;
        }
    }

    private void MakeCultist(EntityUid cultist, Entity<BloodCultRuleComponent> rule)
    {
        if (!_mind.TryGetMind(cultist, out var mindId, out var mind))
            return;

        EnsureComp<BloodCultSpellsHolderComponent>(cultist);

        _faction.RemoveFaction(cultist, rule.Comp.NanoTrasenFaction);
        _faction.AddFaction(cultist, rule.Comp.BloodCultFaction);

        _mind.TryAddObjective(mindId, mind, "KillTargetCultObjective");
    }

    private void GetRandomRunePlacements(BloodCultRuleComponent component)
    {
        var allMarkers = EntityQuery<RendingRunePlacementMarkerComponent>().ToList();
        if (allMarkers.Count == 0)
        {
            component.EmergencyMarkersMode = true;
            component.EmergencyMarkersCount = component.RendingRunePlacementsAmount;
            return;
        }

        var maxRunes = component.RendingRunePlacementsAmount;
        if (allMarkers.Count < component.RendingRunePlacementsAmount)
            maxRunes = allMarkers.Count;

        for (var i = maxRunes; i > 0; i--)
        {
            var marker = _random.PickAndTake(allMarkers);
            marker.IsActive = true;
        }
    }

    private void RemoveAllCultItems(Entity<BloodCultistComponent> cultist)
    {
        if (!_inventory.TryGetContainerSlotEnumerator(cultist.Owner, out var enumerator))
            return;

        _bloodSpear.DetachSpearFromMaster(cultist);
        while (enumerator.MoveNext(out var container))
            if (container.ContainedEntity != null && HasComp<CultItemComponent>(container.ContainedEntity.Value))
                _container.Remove(container.ContainedEntity.Value, container, true, true);

        foreach (var item in _hands.EnumerateHeld(cultist))
            if (TryComp(item, out CultItemComponent? cultItem) && !cultItem.AllowUseToEveryone &&
                !_hands.TryDrop(cultist, item, null, false, false))
                QueueDel(item);
    }

    private void RemoveCultistAppearance(Entity<BloodCultistComponent> cultist)
    {
        if (TryComp<HumanoidAppearanceComponent>(cultist, out var appearanceComponent))
        {
            appearanceComponent.EyeColor = cultist.Comp.OriginalEyeColor;
            Dirty(cultist, appearanceComponent);
        }

        RemComp<PentagramComponent>(cultist);
    }

    private void UpdateCultStage(BloodCultRuleComponent cultRule)
    {
        var cultistsCount = cultRule.Cultists.Count;
        var prevStage = cultRule.Stage;

        if (cultistsCount >= cultRule.PentagramThreshold)
        {
            cultRule.Stage = CultStage.Pentagram;
            SelectRandomLeader(cultRule);
        }
        else if (cultistsCount >= cultRule.ReadEyeThreshold)
            cultRule.Stage = CultStage.RedEyes;
        else
            cultRule.Stage = CultStage.Start;

        if (cultRule.Stage != prevStage)
            UpdateCultistsAppearance(cultRule, prevStage);
    }

    private void UpdateCultistsAppearance(BloodCultRuleComponent cultRule, CultStage prevStage)
    {
        switch (cultRule.Stage)
        {
            case CultStage.Start when prevStage == CultStage.RedEyes:
                foreach (var cultist in cultRule.Cultists)
                    RemoveCultistAppearance(cultist);

                break;
            case CultStage.RedEyes when prevStage == CultStage.Start:
                foreach (var cultist in cultRule.Cultists)
                {
                    if (!TryComp<HumanoidAppearanceComponent>(cultist, out var appearanceComponent))
                        continue;
                    cultist.Comp.OriginalEyeColor = appearanceComponent.EyeColor;
                    appearanceComponent.EyeColor = cultRule.EyeColor;
                    Dirty(cultist, appearanceComponent);
                }

                break;
            case CultStage.Pentagram:
                foreach (var cultist in cultRule.Cultists)
                    EnsureComp<PentagramComponent>(cultist);

                break;
        }
    }

    /// <summary>
    ///     A crutch while we have no NORMAL voting system. The DarkRP one fucking sucks.
    /// </summary>
    private void SelectRandomLeader(BloodCultRuleComponent cultRule)
    {
        if (cultRule.LeaderSelected)
            return;

        var candidats = cultRule.Cultists;
        candidats.RemoveAll(
            entity =>
                TryComp(entity, out PullableComponent? pullable) && pullable.BeingPulled ||
                TryComp(entity, out CuffableComponent? cuffable) && cuffable.CuffedHandCount > 0);

        if (candidats.Count == 0)
            return;

        var leader = _random.Pick(candidats);
        AddComp<BloodCultLeaderComponent>(leader);
        cultRule.LeaderSelected = true;
        cultRule.CultLeader = leader;
    }
}
