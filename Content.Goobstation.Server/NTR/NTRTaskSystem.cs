// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Common.NTR;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.NTR;
using Content.Goobstation.Shared.NTR.Documents;
using Content.Goobstation.Shared.NTR.Events;
using Content.Server.NameIdentifier;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.NameIdentifier;
using Content.Shared.Paper;
using Content.Shared.Store.Components;
using Content.Shared.Tag;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

// Lucifer: goidacore & PURE SHITCODE inside
// pheenty: This is true, I've checked...
namespace Content.Goobstation.Server.NTR;

public sealed class NtrTaskSystem : EntitySystem
{
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IGameTiming _timing = default!; // how did this even happen
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;

    private readonly ProtoId<NameIdentifierGroupPrototype> _nameIdentifierGroup = "Task";

    // TODO: Make calculating & balance update in new methods for less code duplication

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NtrClientAccountComponent, NtrAccountBalanceUpdatedEvent>(OnBalanceUpdated);
        SubscribeLocalEvent<NtrClientAccountComponent, NtrListingPurchaseEvent>(OnPurchase);
        SubscribeLocalEvent<NtrTaskConsoleComponent, BoundUIOpenedEvent>(OnOpened);
        SubscribeLocalEvent<NtrTaskConsoleComponent, TaskPrintLabelMessage>(OnPrintLabelMessage);
        SubscribeLocalEvent<NtrTaskConsoleComponent, TaskSkipMessage>(OnTaskSkipMessage);
        SubscribeLocalEvent<NtrTaskDatabaseComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<NtrTaskConsoleComponent, TaskFailedEvent>(OnTaskFailed);
        SubscribeLocalEvent<NtrTaskConsoleComponent, TaskCompletedEvent>(OnTaskCompleted);

        SubscribeLocalEvent<NtrTaskConsoleComponent, ItemSlotInsertAttemptEvent>(OnInsertAttempt);

        // SubscribeLocalEvent<DocumentInsertedEvent>(OnDocumentInserted);
    }

    private void OnPurchase(EntityUid uid, NtrClientAccountComponent component, NtrListingPurchaseEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<NtrBankAccountComponent>(station, out var ntrAccount))
            return;
        ntrAccount.Balance -= args.Cost.Int();
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NtrTaskDatabaseComponent>();
        while (query.MoveNext(out var uid, out var db))
        {
            foreach (var task in db.Tasks.ToArray())
            {
                if (task.IsActive && (_timing.CurTime - task.ActiveTime) > db.MaxActiveTime)
                {
                    TryRemoveTask(uid, task.Id, true);
                }
            }

            if (_timing.CurTime >= db.NextTaskGenerationTime && db.Tasks.Count < db.MaxTasks)
            {
                if (TryAddTask(uid, db))
                {
                    db.NextTaskGenerationTime = _timing.CurTime + db.TaskGenerationDelay;
                    UpdateTaskConsoles();
                }
            }
        }
    }
#region Task logic

    private void OnInsertAttempt(EntityUid uid, NtrTaskConsoleComponent component, ItemSlotInsertAttemptEvent args)
    { // times this system was re-done: 7 :death:
        var item = args.Item;
        args.Cancelled = true;

        // If one of the checks succeeds, we return
        // do not mess up the order please or it would not work, i beg you
        if (TryHandleSpamDocument(item, uid, component)
        || TryHandleVial(item, uid, component)
        || TryHandleRegularDocument(item, uid, component)) { }
    }

    private bool TryHandleSpamDocument(EntityUid item, EntityUid console, NtrTaskConsoleComponent component)
    {// todo: make this actually work and appear
        if (!HasComp<SpamDocumentComponent>(item))
            return false;

        // _popup.PopupEntity(Loc.GetString("ntr-console-spam-penalty"), console);
        _audio.PlayPvs(component.DenySound, console);

        // var ev = new TaskFailedEvent();
        // RaiseLocalEvent(console, ev);

        return true;
    }

    private bool TryHandleVial(EntityUid item, EntityUid console, NtrTaskConsoleComponent component)
    {
        if (!HasTag(item, "Vial") || !HasComp<SolutionContainerManagerComponent>(item))
            return false;

        var stationEnt = _station.GetOwningStation(console);
        if (stationEnt == null || !TryComp<NtrTaskDatabaseComponent>(stationEnt.Value, out var db))
            return false;
        foreach (var taskData in db.Tasks.ToList())
        {
            if (!taskData.IsActive)
                continue;

            if (!_protoMan.TryIndex(taskData.Task, out var taskProto) || !taskProto.IsReagentTask)
                continue;

            if (CheckReagentRequirements(item, taskProto))
            {
                if (ProcessSuccessfulSubmission(item, console, component, taskProto.ID))
                {
                    return true;
                }
            }
        }

        return false;
    }
    private bool TryHandleRegularDocument(EntityUid item, EntityUid console, NtrTaskConsoleComponent component)
    {
        if (!TryComp<RandomDocumentComponent>(item, out var documentComp) || documentComp.Tasks.Count == 0)
            return false;
        if (!HasValidStamps(item))
        {
            _popup.PopupEntity(_loc.GetString("ntr-console-insert-deny"), console);
            _audio.PlayPvs(component.DenySound, console);
            return true;
        }
        for (var i = 0; i < documentComp.Tasks.Count; i++)
        {
            var taskId = documentComp.Tasks[i];
            if (ProcessSuccessfulSubmission(item, console, component, taskId))
            {
                documentComp.Tasks.RemoveAt(i);
                UpdateTaskConsoles();
                return true;
            }
        }
        return false;
    }
    private bool ProcessSuccessfulSubmission(EntityUid item, EntityUid console, NtrTaskConsoleComponent component, string taskId)
    {
        var stationEnt = _station.GetOwningStation(console);
        if (stationEnt == null)
            return false;

        if (!TryComp<NtrTaskDatabaseComponent>(stationEnt.Value, out var db))
            return false;

        if (!TryComp<NtrBankAccountComponent>(stationEnt.Value, out var account))
            return false;

        if (!_protoMan.TryIndex(taskId, out NtrTaskPrototype? taskProto))
        {
            _popup.PopupEntity(_loc.GetString("ntr-console-task-fail"), console);
            return false;
        }

        if (!TryGetActiveTask(stationEnt.Value, taskProto, out var taskData))
            return false;

        var ev = new TaskCompletedEvent(taskProto);
        RaiseLocalEvent(console, ev);

        if (TryRemoveTask(stationEnt.Value, taskData.Value.Id, false))
        {
            db.History.Add(new NtrTaskHistoryData(
                taskData.Value,
                NtrTaskHistoryData.TaskResult.Completed,
                _gameTiming.CurTime,
                null
            ));
        }
        UpdateClientBalances(account);
        QueueDel(item);
        _audio.PlayPvs(component.PrintSound, console);

        return true;
    }
    private void UpdateClientBalances(NtrBankAccountComponent account)
    {
        var query = EntityQueryEnumerator<NtrClientAccountComponent>();
        while (query.MoveNext(out var client, out _))
        {
            var balanceEv = new NtrAccountBalanceUpdatedEvent(client, account.Balance);
            RaiseLocalEvent(client, balanceEv);
        }
    }
    private bool HasTag(EntityUid item, string tag)
    {
        return EntityManager.System<TagSystem>().HasTag(item, tag);
    }
    private string? GetActorName(EntityUid actor)
    {
        var getIdentityEvent = new TryGetIdentityShortInfoEvent(
            actor,
            actor,
            true
        );
        RaiseLocalEvent(getIdentityEvent);
        return getIdentityEvent.Title;
    }
#endregion
#region reagent shit
    public bool CheckReagentRequirements(EntityUid container, NtrTaskPrototype task)
    {
        if (!_solutionContainer.TryGetSolution(container, task.SolutionName, out _, out var solution))
        {
            _popup.PopupEntity(_loc.GetString("ntr-console-no-solution", ("solutionName", task.SolutionName)), container);
            return false;
        }

        foreach (var (reagentProtoId, requiredAmount) in task.Reagents)
        {
            if (!_protoMan.TryIndex(reagentProtoId, out var requiredReagentProto))
            {
                _popup.PopupEntity(_loc.GetString("ntr-console-invalid-reagent-proto", ("reagentId", reagentProtoId)), container);
                return false;
            }

            var actualAmount = 0;
            var actualReagent = "None";
            foreach (var reagent in solution.Contents)
            {
                if (reagent.Reagent.Prototype != requiredReagentProto.ID)
                    continue;
                actualAmount += (int) (reagent.Quantity * 100);
                actualReagent = reagent.Reagent.Prototype;
            }

            if (actualAmount < requiredAmount)
            {
                _popup.PopupEntity(_loc.GetString("ntr-console-insufficient-reagent-debug",
                        ("requiredReagent", requiredReagentProto.ID),
                        ("actualReagent", actualReagent),
                        ("required", requiredAmount),
                        ("actual", actualAmount)),
                    container);
                return false;
            }
        }

        return true;
    }
#endregion
#region More task logic
    private bool TryGetActiveTask(EntityUid station, NtrTaskPrototype proto, [NotNullWhen(true)] out NtrTaskData? task)
    {
        task = null;
        return TryComp<NtrTaskDatabaseComponent>(station, out var db) &&
            (task = db.Tasks.FirstOrDefault(t =>
                t.Task == proto.ID && t.IsActive)) != null;
    }
    private bool HasValidStamps(EntityUid paper)
    {
        if (!TryComp<PaperComponent>(paper, out var paperComp) ||
            !TryComp<RandomDocumentComponent>(paper, out var documentComp))
            return false;

        var requiredStamps = GetRequiredStamps(documentComp);
        return requiredStamps.Count != 0 && AreStampsCorrect(paperComp, requiredStamps);
    }

    private HashSet<string> GetRequiredStamps(RandomDocumentComponent documentComp)
    {
        var requiredStamps = new HashSet<string>();
        foreach (var taskId in documentComp.Tasks)
        {
            if (!_protoMan.TryIndex(taskId, out var taskProto))
                continue;

            foreach (var entry in taskProto.Entries)
                requiredStamps.UnionWith(entry.Stamps);
        }
        return requiredStamps;
    }
    private bool AreStampsCorrect(PaperComponent paperComp, HashSet<string> requiredStamps)
    {
        if (paperComp.StampedBy.Count == 0 || requiredStamps.Count == 0)
            return false;

        foreach (var requiredStamp in requiredStamps)
        {
            var found = false;
            foreach (var stamp in paperComp.StampedBy)
            {
                if (stamp.StampedName != requiredStamp)
                    continue;
                found = true;
                break;
            }
            if (!found)
                return false;
        }
        return true;
    }
    public bool TryGetTaskId(EntityUid uid, NtrTaskPrototype taskProto, [NotNullWhen(true)] out string? taskId)
    {
        taskId = null;

        if (_station.GetOwningStation(uid) is not { } station || !TryComp<NtrTaskDatabaseComponent>(station, out var db))
            return false;

        foreach (var taskData in db.Tasks)
        {
            if (taskData.Task == taskProto.ID)
            {
                taskId = taskData.Id;
                return true;
            }
        }
        return false;
    }

    private void OnTaskCompleted(EntityUid uid, NtrTaskConsoleComponent component, TaskCompletedEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<NtrTaskDatabaseComponent>(station, out var db))
            return;
        if (!TryGetTaskId(station, args.Task, out _))
            return;
        if (!TryComp<NtrBankAccountComponent>(station, out var ntrAccount))
            return;
        ntrAccount.Balance += args.Task.Reward;
        var query = EntityQueryEnumerator<NtrClientAccountComponent>();
        while (query.MoveNext(out var client, out _))
        {
            var balanceEv = new NtrAccountBalanceUpdatedEvent(client, ntrAccount.Balance);
            RaiseLocalEvent(client, balanceEv);
        }

        if (TryGetTaskId(station, args.Task, out var taskId))
        {
            component.ActiveTaskIds.Remove(taskId);
            TryRemoveTask(station, taskId, false);
        }
        UpdateTaskConsoles();
        var untilNextSkip = db.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(uid,
            NtrTaskUiKey.Key,
            new NtrTaskConsoleState(db.Tasks, db.History, untilNextSkip));
        _audio.PlayPvs(component.SkipSound, uid);
    }

    private void OnPrintLabelMessage(EntityUid uid, NtrTaskConsoleComponent component, TaskPrintLabelMessage args)
    {
        if (_timing.CurTime < component.NextPrintTime)
            return;

        if (component.ActiveTaskIds.Contains(args.TaskId))
        {
            _audio.PlayPvs(component.DenySound, uid);
            return;
        }

        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<NtrTaskDatabaseComponent>(station, out var db))
            return;

        if (!TryGetTaskFromId(station, args.TaskId, out var task))
            return;
        for (var i = 0; i < db.Tasks.Count; i++)
        {
            if (db.Tasks[i].Id != task.Value.Id)
                continue;
            db.Tasks[i] = db.Tasks[i].AsActive(_timing.CurTime);
            break;
        }

        if (!_protoMan.TryIndex(task.Value.Task, out var ntrPrototype))
            return;

        if (ntrPrototype.Entries.Any(e => e.IsEvent))
        {
            var ev = new TaskCompletedEvent(ntrPrototype);
            RaiseLocalEvent(uid, ev);
        }
        var vial = Spawn(ntrPrototype.Proto, Transform(uid).Coordinates);

        if (HasComp<SolutionContainerManagerComponent>(vial))
        {
            if (_solutionContainer.EnsureSolution(vial, "drink", out var beakerSolution, FixedPoint2.New(30)))
            {
                beakerSolution.RemoveAllSolution();
                component.ActiveTaskIds.Add(args.TaskId);
            }
        }
        if (!task.Value.IsActive)
        {
            for (var i = 0; i < db.Tasks.Count; i++)
            {
                if (db.Tasks[i].Id != task.Value.Id)
                    continue;
                db.Tasks[i] = db.Tasks[i].AsActive(_timing.CurTime);
                break;
            }
        }
        component.ActiveTaskIds.Add(args.TaskId);
        component.NextPrintTime = _timing.CurTime + component.PrintDelay;
        _audio.PlayPvs(component.PrintSound, uid);
        UpdateTaskConsoles();
    }
#endregion
#region Console logic

    private void OnBalanceUpdated(EntityUid uid, NtrClientAccountComponent clientComp, ref NtrAccountBalanceUpdatedEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var storeComp))
            return;
        // there was a bug that i couldnt replicate, that made the account ballance
        // go negative, so yeah... shitcod
        var newBalance = Math.Max(0, args.Balance);

        Log.Debug($"Old storeComp.Balance = {storeComp.Balance.First().Value.Value}; args.Balance = {args.Balance}");
        storeComp.Balance["NTLoyaltyPoint"] = FixedPoint2.New(newBalance);
        Log.Debug($"New storeComp.Balance = {storeComp.Balance.First().Value.Value}");
        Dirty(uid, storeComp);
        Log.Debug($"Check {storeComp.Balance.First().Value.Value}");
    }
    private void OnOpened(EntityUid uid, NtrTaskConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<NtrTaskDatabaseComponent>(station, out var taskDb))
            return;

        var untilNextSkip = taskDb.NextSkipTime - _timing.CurTime;
        var state = new NtrTaskConsoleState(
            taskDb.Tasks,
            taskDb.History,
            untilNextSkip,
            new HashSet<string>()
        );
        _uiSystem.SetUiState(uid, NtrTaskUiKey.Key, state);
    }

    private void OnTaskSkipMessage(EntityUid uid, NtrTaskConsoleComponent component, TaskSkipMessage args)
    {
        if (_station.GetOwningStation(uid) is not { } station || !TryComp<NtrTaskDatabaseComponent>(station, out var db))
            return;

        if (_timing.CurTime < db.NextSkipTime)
            return;

        if (!TryGetTaskFromId(station, args.TaskId, out var task))
            return;

        if (args.Actor is not { Valid: true } mob)
            return;

        if (TryComp<AccessReaderComponent>(uid, out var accessReaderComponent) &&
            !_accessReaderSystem.IsAllowed(mob, uid, accessReaderComponent))
        {
            _audio.PlayPvs(component.DenySound, uid);
            return;
        }

        if (!TryRemoveTask(station, task.Value, true, args.Actor))
            return;

        FillTasksDatabase(station);
        db.NextSkipTime = _timing.CurTime + db.SkipDelay;
        var untilNextSkip = db.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(uid, NtrTaskUiKey.Key, new NtrTaskConsoleState(db.Tasks, db.History, untilNextSkip));
        _audio.PlayPvs(component.SkipSound, uid);
    }
    private void OnMapInit(EntityUid uid, NtrTaskDatabaseComponent component, MapInitEvent args)
    {
        //create all max tasks on init
        while (component.Tasks.Count < component.MaxTasks && TryAddTask(uid, component))
        {
            // this supposed to loop this until its the max ammount of tasks
        }

        //set initial generation timer
        component.NextTaskGenerationTime = _timing.CurTime + component.TaskGenerationDelay;
    }
    public void FillTasksDatabase(EntityUid uid, NtrTaskDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        while (component.Tasks.Count < component.MaxTasks)
        {
            if (!TryAddTask(uid, component))
                break;
        }

        UpdateTaskConsoles();
    }

    public void RerollTasksDatabase(Entity<NtrTaskDatabaseComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.Tasks.Clear();
        FillTasksDatabase(entity);
    }

    [PublicAPI]
    public bool TryAddTask(EntityUid uid, NtrTaskDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        // powergridcheck task should not be super common

        var availableTasks = GetAvailableTasks(uid, component);
        if (availableTasks.Count == 0)
            return false;
        var task = PickWeightedTask(availableTasks);
        if (task == null)
            return false;

        if (task.ID == "PowerGridCheck")
        {
            component.NextPowerGridTime = _timing.CurTime + component.PowerGridCooldown;
        }

        return TryAddTask(uid, task, component);
    }
    private List<NtrTaskPrototype> GetAvailableTasks(EntityUid _, NtrTaskDatabaseComponent component)
    {
        var currentTime = _timing.CurTime.TotalSeconds;

        return _protoMan.EnumeratePrototypes<NtrTaskPrototype>()
            .Where(proto =>
                proto.ID != "PowerGridCheck" ||
                (_timing.CurTime >= component.NextPowerGridTime))
            .Where(proto =>
                !component.Tasks.Any(b => b.Task == proto.ID && !b.IsActive) &&
                component.History
                    .Where(h => h.Task == proto.ID)
                    .All(h => (currentTime - h.CompletionTime) >= proto.Cooldown)
            )
            .ToList();
    }
    // weight pick for future tasks
    private NtrTaskPrototype? PickWeightedTask(List<NtrTaskPrototype> tasks)
    {
        if (tasks.Count == 0)
            return null;

        var totalWeight = tasks.Sum(t => t.Weight);
        if (totalWeight <= 0)
            return _random.Pick(tasks);

        var randomValue = _random.NextFloat() * totalWeight;
        var currentSum = 0f;

        foreach (var task in tasks)
        {
            currentSum += task.Weight;
            if (randomValue <= currentSum)
                return task;
        }

        return _random.Pick(tasks);
    }

    public bool TryAddTask(EntityUid uid, NtrTaskPrototype task, NtrTaskDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.Tasks.Count >= component.MaxTasks)
            return false;
        _nameIdentifier.GenerateUniqueName(uid, _nameIdentifierGroup, out var randomVal);
        var newTask = new NtrTaskData(task, randomVal);
        // This bounty id already exists! Probably because NameIdentifierSystem ran out of ids.
        if (component.Tasks.Any(b => b.Id == newTask.Id))
        {
            Log.Error("Failed to add bounty {ID} because another one with the same ID already existed!", newTask.Id);
            return false;
        }
        component.Tasks.Add(new NtrTaskData(task, randomVal));
        _adminLogger.Add(LogType.Action, LogImpact.Low, $"Added bounty \"{task.ID}\" (id:{component.TotalTasks}) to station {ToPrettyString(uid)}");
        component.TotalTasks++;
        return true;
    }

    [PublicAPI]
    public bool TryRemoveTask(Entity<NtrTaskDatabaseComponent?> ent,
        string dataId,
        bool skipped,
        EntityUid? actor = null)
    {
        if (!TryGetTaskFromId(ent.Owner, dataId, out var data, ent.Comp))
            return false;

        return TryRemoveTask(ent, data.Value, skipped, actor);
    }

    /// <summary>
    /// ??? shitcode from bounty system
    /// </summary>
    public bool TryRemoveTask(Entity<NtrTaskDatabaseComponent?> ent,
        NtrTaskData data,
        bool skipped,
        EntityUid? actor = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;
        var removed = ent.Comp.Tasks.RemoveAll(t => t.Id == data.Id) > 0;

        if (removed)
        {
            ent.Comp.History.Add(new NtrTaskHistoryData(
            data,
            skipped ? NtrTaskHistoryData.TaskResult.Skipped
                    : NtrTaskHistoryData.TaskResult.Completed,
            _gameTiming.CurTime,
            actor.HasValue ? GetActorName(actor.Value) : null
        ));
        }

        return removed;
    }
    public bool TryGetTaskFromId(
        EntityUid uid,
        string id,
        [NotNullWhen(true)] out NtrTaskData? task,
        NtrTaskDatabaseComponent? component = null)
    {
        task = null;
        if (!Resolve(uid, ref component))
            return false;

        foreach (var taskData in component.Tasks)
        {
            if (taskData.Id != id)
                continue;
            task = taskData;
            break;
        }

        return task != null;
    }
    private void UpdateTaskConsoles()
    { // refactored this whole thing because i hate myself
        var query = EntityQueryEnumerator<NtrTaskConsoleComponent, UserInterfaceComponent>();
        while (query.MoveNext(out var uid, out var provider, out var ui))
        {
            if (_station.GetOwningStation(uid) is not { } station ||
                !TryComp<NtrTaskDatabaseComponent>(station, out var db))
                continue;

            var untilNextSkip = db.NextSkipTime - _timing.CurTime;
            var state = new NtrTaskConsoleState(
                db.Tasks,
                db.History,
                untilNextSkip,
                provider.ActiveTaskIds
            );
            _uiSystem.SetUiState((uid, ui), NtrTaskUiKey.Key, state);
        }
    }
    private void OnTaskFailed(EntityUid uid, NtrTaskConsoleComponent component, TaskFailedEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<NtrBankAccountComponent>(station, out var ntrAccount))
            return;
        // apply penalty
        ntrAccount.Balance = Math.Max(0, ntrAccount.Balance - args.Penalty);

        var ev = new NtrAccountBalanceUpdatedEvent(uid, ntrAccount.Balance);
        var query = EntityQueryEnumerator<NtrClientAccountComponent>();
        while (query.MoveNext(out var client, out _))
        {
            RaiseLocalEvent(client, ev);
        }
        // del the spam document
        if (Exists(args.User))
            _popup.PopupEntity(_loc.GetString("ntr-console-spam-penalty"), uid, args.User);

        _audio.PlayPvs(component.DenySound, uid);
    }
}
#endregion
