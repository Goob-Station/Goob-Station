// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
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
using Content.Shared.Paper;
using Content.Shared.Store.Components;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.NTR;

public sealed class NtrTaskSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private const string NameIdentifierGroup = "Task";

    public override void Initialize()
    {
        SubscribeLocalEvent<NtrClientAccountComponent, NtrAccountBalanceUpdatedEvent>(OnBalanceUpdated);
        SubscribeLocalEvent<NtrClientAccountComponent, NtrListingPurchaseEvent>(OnPurchase);
        SubscribeLocalEvent<NtrTaskConsoleComponent, BoundUIOpenedEvent>(OnConsoleOpened);
        SubscribeLocalEvent<NtrTaskDatabaseComponent, MapInitEvent>(OnDatabaseInit);
        SubscribeLocalEvent<NtrTaskConsoleComponent, TaskFailedEvent>(OnTaskFailed);
        SubscribeLocalEvent<NtrTaskConsoleComponent, TaskCompletedEvent>(OnTaskCompleted);
        SubscribeLocalEvent<NtrTaskConsoleComponent, ItemSlotInsertAttemptEvent>(OnItemInsertAttempt);
        SubscribeLocalEvent<NtrTaskConsoleComponent, TaskPrintLabelMessage>(OnPrintLabelMessage);
        SubscribeLocalEvent<NtrTaskConsoleComponent, TaskSkipMessage>(OnTaskSkipMessage);
    }

    #region Balance Management
    private void OnBalanceUpdated(EntityUid uid, NtrClientAccountComponent client, ref NtrAccountBalanceUpdatedEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Balance["NTLoyaltyPoint"] = FixedPoint2.New(Math.Max(0, args.Balance));
        Dirty(uid, store);
    }

    private void OnPurchase(EntityUid uid, NtrClientAccountComponent component, NtrListingPurchaseEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station
            || !TryComp<NtrBankAccountComponent>(station, out var account))
            return;

        account.Balance = Math.Max(0, account.Balance - args.Cost.Int());
    }

    private void UpdateAccountBalance(EntityUid station, int delta)
    {
        if (!TryComp<NtrBankAccountComponent>(station, out var account))
            return;

        account.Balance = Math.Max(0, account.Balance + delta);
        RaiseBalanceUpdatedEvent(station, account.Balance);
    }

    private void RaiseBalanceUpdatedEvent(EntityUid station, int newBalance)
    {
        var ev = new NtrAccountBalanceUpdatedEvent(station, newBalance);
        var query = EntityQueryEnumerator<NtrClientAccountComponent>();
        while (query.MoveNext(out var client, out _))
            RaiseLocalEvent(client, ev);
    }

    private void OnPrintLabelMessage(EntityUid uid, NtrTaskConsoleComponent component, TaskPrintLabelMessage args)
    {
        //if (_timing.CurTime < component.NextPrintTime)
        //return;

        //if (component.ActiveTaskIds.Contains(args.TaskId))
        //{
        //    _audio.PlayPvs(component.DenySound, uid);
        //    return;
        //}

        if (_station.GetOwningStation(uid) is not { } station
            || !TryComp<NtrTaskDatabaseComponent>(station, out var db)
            || !TryGetTaskFromId(station, args.TaskId, out var taskData))
            return;

        if (!_prototypes.TryIndex(taskData.Value.Task, out var taskProto))
            return;
        for (int i = 0; i < db.Tasks.Count; i++)
        {
            if (db.Tasks[i].Id == taskData.Value.Id)
            {
                var updated = db.Tasks[i];
                updated.IsActive = true;
                updated.IsAccepted = true;
                updated.ActiveTime = _timing.CurTime;
                db.Tasks[i] = updated;
                break;
            }
        }

        var vial = Spawn(taskProto.Proto, Transform(uid).Coordinates);
        component.ActiveTaskIds.Add(args.TaskId);
        component.NextPrintTime = component.NextSoundTime = _timing.CurTime + component.Delay;
        _audio.PlayPvs(component.PrintSound, uid);
        UpdateTaskConsoles();
    }

    private void OnTaskSkipMessage(EntityUid uid, NtrTaskConsoleComponent component, TaskSkipMessage args)
    {
        if (_station.GetOwningStation(uid) is not { } station
            || !TryComp<NtrTaskDatabaseComponent>(station, out var db)
            || _timing.CurTime < db.NextSkipTime)
            return;

        if (TryRemoveTask(station, args.TaskId, true, args.Actor))
        {
            db.NextSkipTime = _timing.CurTime + db.SkipDelay;
            UpdateConsoleUi(uid, db);
            _audio.PlayPvs(component.SkipSound, uid);
        }
    }
    #endregion

    #region Task Lifecycle
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<NtrTaskDatabaseComponent>();
        while (query.MoveNext(out var uid, out var db))
        {
            CleanExpiredTasks(uid, db);
            GenerateNewTasks(uid, db);
        }
    }

    private void CleanExpiredTasks(EntityUid uid, NtrTaskDatabaseComponent db)
    {
        foreach (var task in db.Tasks.ToArray())
            if (task.IsActive
                && (_timing.CurTime - task.ActiveTime) > db.MaxActiveTime)
                TryRemoveTask(uid, task.Id, true);
    }

    private void GenerateNewTasks(EntityUid uid, NtrTaskDatabaseComponent db)
    {
        while (_timing.CurTime >= db.NextTaskGenerationTime
            && db.Tasks.Count < db.MaxTasks)
        {
            var availableTasks = GetAvailableTasks(db);

            var task = PickWeightedTask(availableTasks);

            if (task == null || !TryAddTask(uid, task, db))
                break;

            db.NextTaskGenerationTime = _timing.CurTime + db.TaskGenerationDelay;
        }
    }

    private void OnTaskCompleted(EntityUid uid, NtrTaskConsoleComponent component, TaskCompletedEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station)
            return;

        HandleTaskOutcome(uid, station, args.Task, success: true);
        _audio.PlayPvs(component.SkipSound, uid);
    }

    private void OnTaskFailed(EntityUid uid, NtrTaskConsoleComponent component, TaskFailedEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station)
            return;

        HandleTaskOutcome(uid, station, args.Task, success: false);
        _audio.PlayPvs(component.DenySound, uid);

        if (Exists(args.User))
            _popup.PopupEntity(Loc.GetString("ntr-console-task-fail-insert"), uid, args.User);
    }

    private void HandleTaskOutcome(EntityUid console, EntityUid station, NtrTaskData taskData, bool success)
    {
        if (!TryComp<NtrTaskDatabaseComponent>(station, out var db)
            || !TryComp<NtrBankAccountComponent>(station, out var account)
            || !_prototypes.TryIndex(taskData.Task, out NtrTaskPrototype? taskProto))
            return;

        var amount = success ? taskProto.Reward : -taskProto.Penalty;
        UpdateAccountBalance(station, amount);
        RaiseBalanceUpdatedEvent(station, account.Balance);

        var index = db.Tasks.FindIndex(t => t.Id == taskData.Id);
        if (index != -1)
        {
            db.Tasks.RemoveAt(index);
            var result = success ? NtrTaskHistoryData.TaskResult.Completed
                : NtrTaskHistoryData.TaskResult.Failed;
            db.History.Add(new NtrTaskHistoryData(
                taskData,
                result,
                _timing.CurTime,
                null
            ));
        }

        UpdateTaskConsoles();
    }
    private bool TryGetActiveTask(EntityUid station, NtrTaskPrototype proto, [NotNullWhen(true)] out NtrTaskData? task)
    {
        task = null;
        return TryComp<NtrTaskDatabaseComponent>(station, out var db)
               && (task = db.Tasks.FirstOrDefault(t => t.Task == proto.ID && t.IsActive)) != null;
    }

    public bool TryGetTaskId(EntityUid uid, NtrTaskPrototype taskProto, [NotNullWhen(true)] out string? taskId)
    {
        taskId = null;
        return TryComp<NtrTaskDatabaseComponent>(uid, out var db)
               && db.Tasks.FirstOrDefault(t => t.Task == taskProto.ID) is {} taskData
               && (taskId = taskData.Id) != null;
    }
    #endregion

    #region Document Processing
    private void OnItemInsertAttempt(EntityUid uid, NtrTaskConsoleComponent component, ItemSlotInsertAttemptEvent args)
    {
        args.Cancelled = true;
        var item = args.Item;

        if (TryHandleVial(item, uid, component))
            return;

        TryHandleRegularDocument(item, uid, component);
    }

    private bool TryHandleVial(EntityUid item, EntityUid console, NtrTaskConsoleComponent component)
    {
        if (!_tag.HasTag(item, "Vial") && !_tag.HasTag(item, "Bottle"))
            return false;

        var station = _station.GetOwningStation(console);
        if (station is null || !TryComp<NtrTaskDatabaseComponent>(station, out var db))
            return false;

        foreach (var task in db.Tasks.Where(t => t.IsActive))
        {
            if (!_prototypes.TryIndex(task.Task, out var proto)
                || !proto.IsReagentTask)
                continue;

            if (ValidateReagentRequirements(item, proto))
                return ProcessTaskSubmission(item, console, proto.ID);
        }

        return false;
    }

    private bool TryHandleRegularDocument(EntityUid item, EntityUid console, NtrTaskConsoleComponent component)
    {
        if (!TryComp<RandomDocumentComponent>(item, out var doc)
            || doc.Tasks.Count == 0)
            return false;

        if (!ValidateDocumentStamps(item))
        {
            _popup.PopupEntity(Loc.GetString("ntr-console-insert-deny-stamps"), console);
            _audio.PlayPvs(component.DenySound, console);
            return true;
        }

        for (var i = 0; i < doc.Tasks.Count; i++)
        {
            if (ProcessTaskSubmission(item, console, doc.Tasks[i]))
            {
                doc.Tasks.RemoveAt(i);
                UpdateTaskConsoles();
                return true;
            }
        }
        return false;
    }

    private bool ProcessTaskSubmission(EntityUid item, EntityUid console, string taskId)
    {
        var station = _station.GetOwningStation(console);
        if (station is null
            || !TryComp<NtrTaskDatabaseComponent>(station, out var db)
            || !_prototypes.TryIndex(taskId, out NtrTaskPrototype? task))
            return false;

        if (!TryGetActiveTask(station.Value, task, out var taskData))
            return false;

        RaiseLocalEvent(console, new TaskCompletedEvent(taskData.Value));

        if (TryRemoveTask(station.Value, taskData.Value.Id, false))
        {
            db.History.Add(new NtrTaskHistoryData(
                taskData.Value,
                NtrTaskHistoryData.TaskResult.Completed,
                _timing.CurTime,
                null
            ));
        }

        QueueDel(item);
        _audio.PlayPvs(Comp<NtrTaskConsoleComponent>(console).PrintSound, console);
        return true;
    }

    private bool ValidateDocumentStamps(EntityUid paper)
    {
        if (!TryComp<PaperComponent>(paper, out var paperComp)
            || !TryComp<RandomDocumentComponent>(paper, out var docComp))
            return false;

        var requiredStamps = GetRequiredStamps(docComp);
        return requiredStamps.Count > 0 && ValidateStampPresence(paperComp, requiredStamps);
    }

    private HashSet<string> GetRequiredStamps(RandomDocumentComponent doc)
    {
        var stamps = new HashSet<string>();
        foreach (var taskId in doc.Tasks)
            if (_prototypes.TryIndex(taskId, out var task))
                stamps.UnionWith(task.Entries.SelectMany(e => e.Stamps));

        return stamps;
    }

    private bool ValidateStampPresence(PaperComponent paper, HashSet<string> requiredStamps)
    {
        return requiredStamps.All(rs =>
            paper.StampedBy.Any(s => s.StampedName.Contains(rs)));
    }
    #endregion

    #region Reagent Handling
    private bool ValidateReagentRequirements(EntityUid container, NtrTaskPrototype task)
    {
        if (!_solutionContainer.TryGetSolution(container, task.SolutionName, out _, out var solution))
        {
            _popup.PopupEntity(Loc.GetString("ntr-console-no-solution", ("solutionName", task.SolutionName)), container);
            return false;
        }

        foreach (var (reagentProtoId, requiredAmount) in task.Reagents)
        {
            if (!_prototypes.TryIndex(reagentProtoId, out var requiredReagentProto))
            {
                _popup.PopupEntity(Loc.GetString("ntr-console-invalid-reagent-proto", ("reagentId", reagentProtoId)), container);
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
                _popup.PopupEntity(Loc.GetString("ntr-console-insufficient-reagent-debug",
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

    #region UI Management
    private void OnConsoleOpened(EntityUid uid, NtrTaskConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (_station.GetOwningStation(uid) is { } station
            && TryComp<NtrTaskDatabaseComponent>(station, out var db))
            UpdateConsoleUi(uid, db);
    }

    private void UpdateConsoleUi(EntityUid console, NtrTaskDatabaseComponent db)
    {
        var state = new NtrTaskConsoleState(
            db.Tasks,
            db.History,
            db.NextSkipTime - _timing.CurTime,
            Comp<NtrTaskConsoleComponent>(console).ActiveTaskIds
        );
        _ui.SetUiState(console, NtrTaskUiKey.Key, state);
    }
    private void UpdateTaskConsoles()
    {
        var query = EntityQueryEnumerator<NtrTaskConsoleComponent, UserInterfaceComponent>();
        while (query.MoveNext(out var uid, out var comp, out var ui))
        {
            if (_station.GetOwningStation(uid) is { } station
                && TryComp<NtrTaskDatabaseComponent>(station, out var db))
                UpdateConsoleUi(uid, db);
        }
    }
    #endregion

    #region Database Management
    private void OnDatabaseInit(EntityUid uid, NtrTaskDatabaseComponent db, MapInitEvent args)
    {
        db.Tasks.Clear();
        db.History.Clear();
        db.NextTaskGenerationTime = _timing.CurTime;

        for (var i = 0; i < db.MaxTasks; i++)
        {
            var available = GetAvailableTasks(db);
            var task = PickWeightedTask(available);
            if (task == null
                || !TryAddTask(uid, task, db))
                break;
        }

        UpdateTaskConsoles();
    }

    public bool TryAddTask(EntityUid uid, NtrTaskPrototype task, NtrTaskDatabaseComponent? db = null)
    {
        if (!Resolve(uid, ref db)
            || db.Tasks.Count >= db.MaxTasks
            || string.IsNullOrEmpty(task.ID))
            return false;

        _nameIdentifier.GenerateUniqueName(uid, NameIdentifierGroup, out var id);
        var newId = id.ToString();

        if (db.Tasks.Any(b => b.Id == newId))
            return false;
        var stringId = id.ToString();

        db.Tasks.Add(new NtrTaskData(task, stringId) {
            IsActive = true,
            ActiveTime = _timing.CurTime
        });

        return true;
    }
    private List<NtrTaskPrototype> GetAvailableTasks(NtrTaskDatabaseComponent db)
    {
        return _prototypes.EnumeratePrototypes<NtrTaskPrototype>()
            .Where(proto => IsTaskAvailable(proto, db))
            .ToList();
    }

    private bool IsTaskAvailable(NtrTaskPrototype proto, NtrTaskDatabaseComponent db)
    {
        var currentTime = _timing.CurTime.TotalSeconds;
        return !db.Tasks.Any(b => b.Task == proto.ID && b.IsActive)
               && db.History.Where(h => h.Task == proto.ID)
                   .All(h => (currentTime - h.CompletionTime) >= proto.Cooldown);
    }


    private NtrTaskPrototype? PickWeightedTask(List<NtrTaskPrototype> tasks)
    {
        if (tasks.Count == 0) return null;

        var total = tasks.Sum(t => t.Weight);
        var roll = _random.NextFloat() * total;
        var current = 0f;

        foreach (var task in tasks)
        {
            current += task.Weight;
            if (roll <= current)
                return task;
        }
        return _random.Pick(tasks);
    }

    public bool TryRemoveTask(EntityUid uid, string taskId, bool skipped, EntityUid? actor = null)
    {
        return TryComp<NtrTaskDatabaseComponent>(uid, out var db)
            && TryGetTaskFromId(uid, taskId, out var task, db)
            && TryRemoveTask(uid, task.Value, skipped, actor);
    }

    private bool TryRemoveTask(EntityUid uid, NtrTaskData task, bool skipped, EntityUid? actor = null)
    {
        if (!TryComp<NtrTaskDatabaseComponent>(uid, out var db))
            return false;

        var removed = db.Tasks.RemoveAll(t => t.Id == task.Id) > 0;
        if (removed)
        {
            db.History.Add(new NtrTaskHistoryData(
                task,
                skipped ? NtrTaskHistoryData.TaskResult.Skipped
                    : NtrTaskHistoryData.TaskResult.Completed,
                _timing.CurTime,
                actor.HasValue ? GetActorName(actor.Value) : null
            ));
        }
        return removed;
    }

    public bool TryGetTaskFromId(EntityUid uid, string id, [NotNullWhen(true)] out NtrTaskData? task, NtrTaskDatabaseComponent? db = null)
    {
        task = null;
        return Resolve(uid, ref db)
            && (task = db.Tasks.FirstOrDefault(t => t.Id == id)) != null;
    }
    #endregion

    #region Utility Methods

    private string? GetActorName(EntityUid actor)
    {
        return Identity.Name(actor, EntityManager);
    }
    #endregion
}

public sealed class TaskFailedEvent : EntityEventArgs
{
    public EntityUid User;
    public NtrTaskData Task;
    public int Penalty;

    public TaskFailedEvent(EntityUid user, NtrTaskData task, int penalty)
    {
        User = user;
        Task = task;
        Penalty = penalty;
    }
}
