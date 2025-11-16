using Content.Goobstation.Shared.CorticalBorer;
using Content.Goobstation.Shared.CorticalBorer.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Medical;
using Content.Server.Medical.Components;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Chat;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Database;
using Content.Shared.Inventory;
using Content.Shared.MedicalScanner;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Species.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.CorticalBorer;

public sealed partial class CorticalBorerSystem : SharedCorticalBorerSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly HealthAnalyzerSystem _analyzer = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly GhostRoleSystem _ghost  = default!;
    [Dependency] private readonly CollectiveMindUpdateSystem _collective = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAbilities();

        SubscribeLocalEvent<CorticalBorerComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<CorticalBorerComponent, CorticalBorerDispenserInjectMessage>(OnInjectReagentMessage);
        SubscribeLocalEvent<CorticalBorerComponent, CorticalBorerDispenserSetInjectAmountMessage>(OnSetInjectAmountMessage);

        SubscribeLocalEvent<InventoryComponent, InfestHostAttempt>(OnInfestHostAttempt);
        SubscribeLocalEvent<CorticalBorerComponent, CheckTargetedSpeechEvent>(OnSpeakEvent);

        SubscribeLocalEvent<CorticalBorerComponent, MindRemovedMessage>(OnMindRemoved);
    }

    private void OnMapInit(Entity<CorticalBorerComponent> ent, ref MapInitEvent args)
    {
        Alerts.ShowAlert(ent, ent.Comp.ChemicalAlert);
        UpdateUiState(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var borerQuery = EntityQueryEnumerator<CorticalBorerComponent>();
        while (borerQuery.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateCooldown;

            if (comp.Host.HasValue)
                UpdateChems((uid, comp), comp.ChemicalGenerationRate);
        }

        var infestedQuery = EntityQueryEnumerator<CorticalBorerInfestedComponent>();
        while (infestedQuery.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.ControlTimeEnd)
                EndControl((uid, comp));
        }
    }

    private void OnSpeakEvent(Entity<CorticalBorerComponent> ent, ref CheckTargetedSpeechEvent args)
    {
        args.ChatTypeIgnore.Add(InGameICChatType.CollectiveMind);

        if (!ent.Comp.Host.HasValue)
            return;

        args.Targets.Add(ent);
        args.Targets.Add(ent.Comp.Host.Value);
    }

    public void OnInfestHostAttempt(Entity<InventoryComponent> entity, ref InfestHostAttempt args)
    {
        if (!_inventory.TryGetSlotEntity(entity.Owner, "head", out var headUid) ||
            !TryComp(headUid, out IngestionBlockerComponent? blocker) ||
            !blocker.Enabled)
            return;

        args.Blocker = headUid;
        args.Cancel();
    }

    /// <summary>
    /// Attempts to inject the Borer's host with chems
    /// </summary>
    public bool TryInjectHost(Entity<CorticalBorerComponent> ent,
        CorticalBorerChemicalPrototype chemicalPrototype,
        float chemAmount)
    {
        var (uid, comp) = ent;

        // Need a host to inject something
        if (!comp.Host.HasValue)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), uid, uid, PopupType.Medium);
            return false;
        }

        // Sugar block from injecting stuff
        if (!CanUseAbility(ent, comp.Host.Value))
            return false;

        // Make sure you can even hold the amount of chems you need
        if (chemicalPrototype.Cost > comp.ChemicalPointCap)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-not-enough-chem-storage"), uid, uid, PopupType.Medium);
            return false;
        }

        // Make sure you have enough chems
        if (chemicalPrototype.Cost > comp.ChemicalPoints)
        {
            Popup.PopupEntity(Loc.GetString("cortical-borer-not-enough-chem"), uid, uid, PopupType.Medium);
            return false;
        }

        // no injecting things that don't have blood silly
        if (!TryComp<BloodstreamComponent>(comp.Host, out var bloodstream))
            return false;

        var solution = new Solution();
        solution.AddReagent(chemicalPrototype.Reagent, chemAmount);

        // add the chemicals to the bloodstream of the host
        if (!_blood.TryAddToChemicals((comp.Host.Value, bloodstream), solution))
            return false;

        UpdateChems(ent, -((int)chemAmount * chemicalPrototype.Cost));
        return true;
    }

    private void OnInjectReagentMessage(Entity<CorticalBorerComponent> ent, ref CorticalBorerDispenserInjectMessage message)
    {
        CorticalBorerChemicalPrototype? chemProto = null;
        foreach (var chem in _proto.EnumeratePrototypes<CorticalBorerChemicalPrototype>())
        {
            if (!chem.Reagent.Equals(message.ChemProtoId))
                continue;

            chemProto = chem;
            break;
        }

        if (chemProto != null)
            TryInjectHost(ent, chemProto, ent.Comp.InjectAmount);

        UpdateUiState(ent);
    }

    private void OnSetInjectAmountMessage(Entity<CorticalBorerComponent> ent, ref CorticalBorerDispenserSetInjectAmountMessage message)
    {
        ent.Comp.InjectAmount = message.CorticalBorerDispenserDispenseAmount;
        UpdateUiState(ent);
    }

    public bool TryToggleCheckBlood(Entity<CorticalBorerComponent> ent)
    {
        if (!TryComp<UserInterfaceComponent>(ent, out var uic))
            return false;

        if (!TryComp<HealthAnalyzerComponent>(ent, out var health))
            return false;

        // If open - close
        if (UI.IsUiOpen((ent, uic), HealthAnalyzerUiKey.Key))
        {
            UI.CloseUi((ent, uic), HealthAnalyzerUiKey.Key, ent.Owner);
            if (health.ScannedEntity.HasValue)
                _analyzer.StopAnalyzingEntity((ent, health), health.ScannedEntity.Value);
            return true;
        }

        if (!ent.Comp.Host.HasValue || !TryComp<BloodstreamComponent>(ent.Comp.Host.Value, out _))
            return false;

        UI.OpenUi((ent, uic), HealthAnalyzerUiKey.Key, ent.Owner);
        _analyzer.BeginAnalyzingEntity((ent, health), ent.Comp.Host.Value);

        return true;
    }

    public void TakeControlHost(Entity<CorticalBorerComponent> ent, CorticalBorerInfestedComponent infestedComp)
    {
        var (worm, comp) = ent;

        if (comp.Host is not { } host)
            return;

        // make sure they aren't dead, would throw the worm into a ghost mode and just kill em
        if (TryComp<MobStateComponent>(host, out var mobState) &&
            mobState.CurrentState == MobState.Dead)
            return;

        if (TryComp<MindContainerComponent>(host, out var mindContainer) &&
            mindContainer.HasMind ||
            HasComp<GhostRoleComponent>(host))
            infestedComp.ControlTimeEnd = _timing.CurTime + comp.ControlDuration;

        if (_mind.TryGetMind(worm, out var wormMind, out _))
            infestedComp.BorerMindId = wormMind;

        if (_mind.TryGetMind(host, out var controledMind, out _))
        {
            infestedComp.OriginalMindId = controledMind; // set this var here just in case somehow the mind changes from when the infestation started

            // fish head...
            var dummy = Spawn("FoodMeatFish", MapCoordinates.Nullspace);
            Container.Insert(dummy, infestedComp.ControlContainer);

            _mind.TransferTo(controledMind, dummy);

            Popup.PopupEntity(Loc.GetString("racortical-borer-lost-control"), dummy, dummy, PopupType.LargeCaution);
        }
        else
        {
            infestedComp.OriginalMindId = null;
        }

        comp.ControlingHost = true;
        _mind.TransferTo(wormMind, host);

        if (TryComp<GhostRoleComponent>(worm, out var ghostRole))
            _ghost.UnregisterGhostRole((worm, ghostRole)); // prevent players from taking the worm role once mind isn't in the worm

        // add the end control and vomit egg action
        if (Actions.AddAction(host, ent.Comp.EndControlAction) is {} actionEnd)
            infestedComp.RemoveAbilities.Add(actionEnd);
        if (comp.CanReproduce &&
            infestedComp.ControlTimeEnd != null) // you can't lay eggs with something you can control forever
        {
            if (Actions.AddAction(host, ent.Comp.LayEggAction) is {} actionLay)
                infestedComp.RemoveAbilities.Add(actionLay);
        }

        if (TryComp<ReformComponent>(host, out var reformComp) && reformComp.ActionEntity.HasValue)
        {
            infestedComp.RemovedReformAction = reformComp.ActionEntity.Value;

            Actions.RemoveAction(host, reformComp.ActionEntity.Value);
        }

        // add collective mind if we don't have it already
        var channel = ent.Comp.HivemindChannel;
        var hadHivemind = _collective.HasCollectiveMind(host, channel);
        infestedComp.HadHivemind = hadHivemind;
        if (TryComp<CollectiveMindComponent>(host, out var collectiveComp))
            infestedComp.OldDefault = collectiveComp.DefaultChannel;
        _collective.AddCollectiveMind(host, channel, true); // also set default

        var str = $"{ToPrettyString(worm)} has taken control over {ToPrettyString(host)}";

        Log.Info(str);
        _admin.Add(LogType.Mind, LogImpact.High, $"{ToPrettyString(worm)} has taken control over {ToPrettyString(host)}");
        _chat.SendAdminAlert(str);
    }

    public void EndControl(Entity<CorticalBorerInfestedComponent> host)
    {
        var (infested, infestedComp) = host;

        if (!TryComp<CorticalBorerComponent>(infestedComp.Borer, out var borerComp))
            return;

        if (!borerComp.ControlingHost)
            return;

        borerComp.ControlingHost = false;

        // remove all the actions set to remove
        foreach (var ability in infestedComp.RemoveAbilities)
        {
            Actions.RemoveAction(infested, ability);
        }
        infestedComp.RemoveAbilities = new(); // clear out the list

        if (infestedComp.RemovedReformAction.HasValue && TryComp<ReformComponent>(host, out var reformComp))
        {
            var restoredAction = Actions.AddAction(host, reformComp.ActionPrototype);

            if (restoredAction != null)
            {
                reformComp.ActionEntity = restoredAction.Value;
            }

            infestedComp.RemovedReformAction = null;
        }

        if (TryComp<GhostRoleComponent>(infestedComp.Borer, out var ghostRole))
            _ghost.RegisterGhostRole((infestedComp.Borer, ghostRole)); // re-enable the ghost role after you return to the body

        // Return everyone to their own bodies
        if (!TerminatingOrDeleted(infestedComp.BorerMindId))
            _mind.TransferTo(infestedComp.BorerMindId, infestedComp.Borer);
        if (!TerminatingOrDeleted(infestedComp.OriginalMindId) && infestedComp.OriginalMindId.HasValue)
            _mind.TransferTo(infestedComp.OriginalMindId.Value, infested);

        if (!infestedComp.HadHivemind)
            _collective.RemoveCollectiveMind(infested, borerComp.HivemindChannel);
        if (TryComp<CollectiveMindComponent>(host, out var collectiveComp))
            collectiveComp.DefaultChannel = infestedComp.OldDefault;

        infestedComp.ControlTimeEnd = null;
        Container.CleanContainer(infestedComp.ControlContainer);
    }

    private void OnMindRemoved(Entity<CorticalBorerComponent> ent, ref MindRemovedMessage args)
    {
        if (!ent.Comp.ControlingHost)
            TryEjectBorer(ent); // No storing them in hosts if you don't have a soul
    }
}
