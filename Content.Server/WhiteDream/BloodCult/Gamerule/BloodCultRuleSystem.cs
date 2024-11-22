using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Hands.Systems;
using Content.Server.Language;
using Content.Server.NPC.Systems;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.StationEvents.Components;
using Content.Server.WhiteDream.BloodCult.Items.BloodSpear;
using Content.Server.WhiteDream.BloodCult.Objectives;
using Content.Server.WhiteDream.BloodCult.Spells;
using Content.Shared.Body.Systems;
using Content.Shared.Cloning;
using Content.Shared.Cuffs.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mood;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Roles;
using Content.Shared.WhiteDream.BloodCult.Components;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Items;
using Robust.Server.Containers;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.WhiteDream.BloodCult.Gamerule;

public sealed class BloodCultRuleSystem : GameRuleSystem<BloodCultRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;
    [Dependency] private readonly BloodSpearSystem _bloodSpear = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly LanguageSystem _languageSystem = default!;
    [Dependency] private readonly NpcFactionSystem _factionSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;

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

        component.OfferingTarget = FindTarget();
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

    private void AfterEntitySelected(Entity<BloodCultRuleComponent> ent, ref AfterAntagEntitySelectedEvent args) =>
        MakeCultist(args.EntityUid, ent);

    private void OnNarsieSummon(BloodCultNarsieSummoned ev)
    {
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var cult, out _))
        {
            cult.WinCondition = CultWinCondition.Win;
            _roundEndSystem.EndRound();

            foreach (var ent in cult.Cultists)
            {
                if (Deleted(ent.Owner) || !TryComp(ent.Owner, out MindContainerComponent? mindContainer) ||
                    !mindContainer.Mind.HasValue)
                    continue;

                var harvester = Spawn(cult.HarvesterPrototype, Transform(ent.Owner).Coordinates);
                _mindSystem.TransferTo(mindContainer.Mind.Value, harvester);
                _bodySystem.GibBody(ent);
            }

            return;
        }
    }

    private void OnCultistComponentInit(Entity<BloodCultistComponent> cultist, ref ComponentInit args)
    {
        RaiseLocalEvent(cultist, new MoodEffectEvent("CultFocused"));
        _languageSystem.AddLanguage(cultist, cultist.Comp.CultLanguageId);

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

        CheckRoundShouldEnd();

        if (TerminatingOrDeleted(cultist.Owner))
            return;

        RemoveAllCultItems(cultist);
        RemoveCultistAppearance(cultist);
        RemoveObjectiveAndRole(cultist.Owner);
        RaiseLocalEvent(cultist.Owner, new MoodRemoveEffectEvent("CultFocused"));
        _languageSystem.RemoveLanguage(cultist.Owner, cultist.Comp.CultLanguageId);

        if (!TryComp(cultist, out BloodCultSpellsHolderComponent? powersHolder))
            return;

        foreach (var power in powersHolder.SelectedSpells)
            _actions.RemoveAction(cultist.Owner, power);
    }

    private void OnCultistsStateChanged(Entity<BloodCultistComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            CheckRoundShouldEnd();
    }

    private void OnClone(Entity<BloodCultistComponent> ent, ref CloningEvent args) => RemoveObjectiveAndRole(ent);

    private void OnGetBriefing(Entity<BloodCultistRoleComponent> ent, ref GetBriefingEvent args) =>
        args.Append(Loc.GetString("blood-cult-role-briefing-short"));

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
                continue;

            _antagSelection.MakeAntag(antagSelectionEnt, actor.PlayerSession, def.Value);
        }
    }

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
        while (query.MoveNext(out _, out var bloodCultRule, out _))
            return entityUid == bloodCultRule.OfferingTarget;

        return false;
    }

    public void RemoveObjectiveAndRole(EntityUid uid)
    {
        if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind))
            return;

        var objectives = mind.Objectives.FindAll(HasComp<KillTargetCultComponent>);
        foreach (var obj in objectives)
            _mindSystem.TryRemoveObjective(mindId, mind, mind.Objectives.IndexOf(obj));

        if (_roleSystem.MindHasRole<BloodCultistRoleComponent>(mindId))
            _roleSystem.MindRemoveRole<BloodCultistRoleComponent>(mindId);
    }

    private void CheckRoundShouldEnd()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            var aliveCultists = cult.Cultists.Count(cultist => !_mobStateSystem.IsDead(cultist));
            if (aliveCultists != 0)
                return;

            cult.WinCondition = CultWinCondition.Failure;

            // Check for all at once gamemode
            if (!GameTicker.GetActiveGameRules().Where(HasComp<RampingStationEventSchedulerComponent>).Any())
                _roundEndSystem.EndRound();
        }
    }

    private void MakeCultist(EntityUid cultist, Entity<BloodCultRuleComponent> rule)
    {
        if (!_mindSystem.TryGetMind(cultist, out var mindId, out var mind))
            return;

        EnsureComp<BloodCultSpellsHolderComponent>(cultist);

        _factionSystem.RemoveFaction(cultist, rule.Comp.NanoTrasenFaction);
        _factionSystem.AddFaction(cultist, rule.Comp.BloodCultFaction);

        _mindSystem.TryAddObjective(mindId, mind, "KillTargetCultObjective");
    }

    private EntityUid? FindTarget(ICollection<EntityUid> exclude = null!)
    {
        var querry = EntityManager
            .EntityQueryEnumerator<MindContainerComponent, HumanoidAppearanceComponent, ActorComponent>();

        var potentialTargets = new List<EntityUid>();

        while (querry.MoveNext(out var uid, out var mind, out _, out _))
        {
            var entity = mind.Mind;
            if (entity == default || exclude?.Contains(uid) is true || HasComp<BloodCultistComponent>(uid))
                continue;

            potentialTargets.Add(uid);
        }

        return potentialTargets.Count > 0 ? _random.Pick(potentialTargets) : null;
    }

    private void RemoveAllCultItems(Entity<BloodCultistComponent> cultist)
    {
        if (!_inventorySystem.TryGetContainerSlotEnumerator(cultist.Owner, out var enumerator))
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
