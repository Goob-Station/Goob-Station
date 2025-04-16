using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Common.NTR;
using Content.Goobstation.Shared.NTR;
using Content.Goobstation.Shared.NTR.Documents;
using Content.Goobstation.Shared.NTR.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Server.NameIdentifier;
using Content.Server.Popups;
using Content.Server.Radio.EntitySystems;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.NameIdentifier;
using Content.Shared.Paper;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;

// goidacore & PURE SHITCODE inside
namespace Content.Goobstation.Server.NTR;

public sealed partial class NtrTaskSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSys = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IGameTiming _timing = default!; // how did this even happen
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _linker = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ShuttleConsoleSystem _console = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaSystem = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

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
                    TryRemoveTask(uid, task, true);
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
    private void OnInsertAttempt(EntityUid uid, NtrTaskConsoleComponent component, ItemSlotInsertAttemptEvent args)
    {
        var item = args.Item;

        ent.Comp.History.Add(new NtrTaskHistoryData(
            data,
            skipped ? TaskResult.Skipped : TaskResult.Completed,
            _gameTiming.CurTime,
            actorName,
            _timing.CurTime.TotalSeconds
        ));

        if (HasComp<SpamDocumentComponent>(item))
        {
            args.Cancelled = true;
            _popup.PopupEntity(Loc.GetString("ntr-console-spam-penalty"), uid);
            return;
        }

        if (!TryComp<RandomDocumentComponent>(item, out var documentComp))
        {
            args.Cancelled = true;
            return;
        }
        if (!HasValidStamps(item))
        {
            args.Cancelled = true;
            _popup.PopupEntity(Loc.GetString("ntr-console-insert-deny"), uid);
            _audio.PlayPvs(component.DenySound, uid);
            return;
        }
        foreach (var taskId in documentComp.Tasks)
        {
            if (!_protoMan.TryIndex(taskId, out NtrTaskPrototype? taskProto))
                continue;

            if (!CheckReagentRequirements(item, taskProto))
            {
                args.Cancelled = true;
                _popup.PopupEntity(Loc.GetString("ntr-console-reagent-fail"), uid);
                return;
            }
        }
        if (!args.Cancelled)
        {
            if (_station.GetOwningStation(uid) is { } station &&
                TryComp<NtrTaskDatabaseComponent>(station, out var db) &&
                TryComp<NtrBankAccountComponent>(station, out var account))
            {
                foreach (var taskId in documentComp.Tasks)
                {
                    if (!_protoMan.TryIndex(taskId, out NtrTaskPrototype? taskProto) ||
                        !TryGetActiveTask(station, taskProto, out var taskData))
                        continue;

                    var ev = new TaskCompletedEvent(taskProto);
                    RaiseLocalEvent(uid, ev);
                    TryRemoveTask(station, taskData.Value.Id, false);
                }

                var query = EntityQueryEnumerator<NtrClientAccountComponent>();
                while (query.MoveNext(out var client, out _))
                {
                    var balanceEv = new NtrAccountBalanceUpdatedEvent(client, account.Balance);
                    RaiseLocalEvent(client, balanceEv);
                }

                UpdateTaskConsoles();
            }

            _slots.TryEject(uid, args.Slot.Name, null, out _);
            QueueDel(item);
            _audio.PlayPvs(component.PrintSound, uid);
        }
    }
    public bool CheckReagentRequirements(EntityUid container, NtrTaskPrototype task)
    {
        if (task.RequiredReagents.Count == 0)
            return true;

        if (!TryComp<SolutionContainerManagerComponent>(container, out var solutionManager))
            return false;

        if (!_solutionContainer.TryGetSolution(container, "beaker", out var solution))
            return false;

        foreach (var (reagentId, requiredAmount) in task.RequiredReagents)
        {
            var actualAmount = _solutionContainer.GetTotalPrototypeQuantity(solution.Value, reagentId);
            if (actualAmount < requiredAmount)
                return false;
        }

        return true;
    }
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
            if (!_protoMan.TryIndex(taskId, out NtrTaskPrototype? taskProto))
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
            bool found = false;
            foreach (var stamp in paperComp.StampedBy)
            {
                if (stamp.StampedName == requiredStamp)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;
        }
        return true;
    }
    private void OnBalanceUpdated(EntityUid uid, NtrClientAccountComponent clientComp, ref NtrAccountBalanceUpdatedEvent args)

    {
        if (!TryComp<StoreComponent>(uid, out var storeComp))
            return;
        Log.Debug($"Old storeComp.Balance = {storeComp.Balance.First().Value.Value}; args.Balance = {args.Balance}");
        storeComp.Balance["NTLoyaltyPoint"] = FixedPoint2.New(args.Balance);
        Log.Debug($"New storeComp.Balance = {storeComp.Balance.First().Value.Value}");
        Dirty(uid, storeComp);
        Log.Debug($"Check {storeComp.Balance.First().Value.Value}");
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
        if (!TryGetTaskId(station, args.Task, out var taskData))
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
        _uiSystem.SetUiState(uid, NtrTaskUiKey.Key,
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
        for (int i = 0; i < db.Tasks.Count; i++)
        {
            if (db.Tasks[i].Id == task.Value.Id)
            {
                db.Tasks[i] = db.Tasks[i].AsActive(_timing.CurTime);
                break;
            }
        }

        if (!_protoMan.TryIndex(task.Value.Task, out var ntrPrototype))
            return;

        if (ntrPrototype.Entries.Any(e => e.IsEvent))
        {
            var ev = new TaskCompletedEvent(ntrPrototype);
            RaiseLocalEvent(uid, ev);
        }
        var vial = Spawn(ntrPrototype.Proto, Transform(uid).Coordinates);

        if (TryComp<SolutionContainerManagerComponent>(vial, out var solutions))
        {
            if (_solutionContainer.EnsureSolution(vial, "beaker", out var beakerSolution, FixedPoint2.New(30)))
            {
                beakerSolution.RemoveAllSolution();
                component.ActiveTaskIds.Add(args.TaskId);
            }
        }

        // TryRemoveTask(station, task.Value, false);
        component.ActiveTaskIds.Add(args.TaskId);
        component.NextPrintTime = _timing.CurTime + component.PrintDelay;
        _audio.PlayPvs(component.PrintSound, uid);
        UpdateTaskConsoles();
    }


    /// <summary>
    /// При открытии интерфейса получаем станцию через GetOwningStation, из станции вытаскиваем компонент базы данных задач НТР
    /// Вычисляем время до скипа задачи, поднимаем метод UpdateState в NtrTaskBoundUserInterface чтобы данные интерфейса изменились
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
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

    /// <summary>
    /// Поднимается при нажатии кнопки Skip, получаем станцию, базу данных, задачу с которой работаем.
    /// AccessReader не трогал, но скорее всего просто установить у прототипа уровень доступа НТРа
    /// Удаляем задачу, заполняем базу данных методом FillTaskDatabase новой случайной задачей.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
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
        var state = new NtrTaskConsoleState(
            db.Tasks,
            db.History,
            untilNextSkip,
            component.ActiveTaskIds
        );
        _uiSystem.SetUiState(uid, NtrTaskUiKey.Key, new NtrTaskConsoleState(db.Tasks, db.History, untilNextSkip));
        _audio.PlayPvs(component.SkipSound, uid);
    }

    /// <summary>
    /// При начале раунда и т.п заполняем БД задачами
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
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
    /// <summary>
    /// Заполняет БД задачами.
    /// </summary>
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

    /// <summary>
    /// Абсолютно новые задачи в БД
    /// </summary>
    /// <param name="entity"></param>
    public void RerollTasksDatabase(Entity<NtrTaskDatabaseComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.Tasks.Clear();
        FillTasksDatabase(entity);
    }

    /// <summary>
    /// Логика для добавления задачи в БД. Берём все прототипы задач и добавляем рандомные
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    [PublicAPI]
    public bool TryAddTask(EntityUid uid, NtrTaskDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        // powergridcheck task should not be super common
        var allTasks = _protoMan.EnumeratePrototypes<NtrTaskPrototype>()
            .Where(proto => proto.ID != "PowerGridCheck" ||
                            _timing.CurTime >= component.NextPowerGridTime)
            .ToList();

        var filteredTasks = new List<NtrTaskPrototype>();
        foreach (var proto in allTasks)
        {
            if (component.Tasks.Any(b => b.Task == proto.ID))
                continue;
            filteredTasks.Add(proto);
        }

        var pool = filteredTasks.Count == 0 ? allTasks : filteredTasks;
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
    private List<NtrTaskPrototype> GetAvailableTasks(EntityUid uid, NtrTaskDatabaseComponent component)
    {
        var currentTime = _timing.CurTime.TotalSeconds;

        return _protoMan.EnumeratePrototypes<NtrTaskPrototype>()
            .Where(proto =>
                proto.ID != "PowerGridCheck" ||
                (_timing.CurTime >= component.NextPowerGridTime))
            .Where(proto =>
                !component.Tasks.Any(b => b.Task == proto.ID && !b.IsActive) &&
                component.History
                    .Where(h => h.TaskData.Task == proto.ID)
                    .All(h => (currentTime - h.CompletionTime) >= proto.Cooldown)
            )
            .ToList();
    }
    // weight pick for future tasks
    private NtrTaskPrototype? PickWeightedTask(List<NtrTaskPrototype> tasks)
    {
        if (tasks.Count == 0)
            return null;

        float totalWeight = tasks.Sum(t => t.Weight);
        if (totalWeight <= 0)
            return _random.Pick(tasks);

        float randomValue = _random.NextFloat() * totalWeight;
        float currentSum = 0;

        foreach (var task in tasks)
        {
            currentSum += task.Weight;
            if (randomValue <= currentSum)
                return task;
        }

        return _random.Pick(tasks);
    }

    /// <summary>
    /// Создаёт рандомный айди для задачи, добавляет задачу в список задач
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="task"></param>
    /// <param name="component"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Снова проверка
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="dataId"></param>
    /// <param name="skipped"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
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
    /// ???
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="data"></param>
    /// <param name="skipped"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public bool TryRemoveTask(Entity<NtrTaskDatabaseComponent?> ent,
        NtrTaskData data,
        bool skipped,
        EntityUid? actor = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        for (var i = 0; i < ent.Comp.Tasks.Count; i++)
        {
            if (ent.Comp.Tasks[i].Id == data.Id)
            {
                string? actorName = null;
                if (actor != null)
                {
                    var getIdentityEvent = new TryGetIdentityShortInfoEvent(ent.Owner, actor.Value);
                    RaiseLocalEvent(getIdentityEvent);
                    actorName = getIdentityEvent.Title;
                }

                ent.Comp.History.Add(new NtrTaskHistoryData(data,
                    skipped
                        ? NtrTaskHistoryData.TaskResult.Skipped
                        : NtrTaskHistoryData.TaskResult.Completed,
                    _gameTiming.CurTime,
                    actorName));
                ent.Comp.Tasks.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Получение задачи по её айди
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="id"></param>
    /// <param name="task"></param>
    /// <param name="component"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Преднамеренно обновляем интерфейс
    /// </summary>
    private void UpdateTaskConsoles()
    { // refactored this whole thing because i hate myself
        var query = EntityQueryEnumerator<NtrTaskConsoleComponent, UserInterfaceComponent>();
        while (query.MoveNext(out var uid, out var provider, out var ui))
        {
            if (_station.GetOwningStation(uid) is not { } station ||
                !TryComp<NtrTaskDatabaseComponent>(station, out var db))
                continue;

            var filteredTasks = db.Tasks
                .Where(t => !provider.ActiveTaskIds.Contains(t.Id))
                .ToList();

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
        while (query.MoveNext(out var client, out var comp))
        {
            RaiseLocalEvent(client, ev);
        }
        // del the spam document
        if (Exists(args.User))
            _popup.PopupEntity(Loc.GetString("ntr-console-spam-penalty"), uid, args.User);

        _audio.PlayPvs(component.DenySound, uid);
    }
    // private void OnDocumentInserted(DocumentInsertedEvent args)
    // {
    //     if (!Exists(args.Document))
    //         return;
    //
    //     if (!TryComp<RandomDocumentComponent>(args.Document, out var documentComp) ||
    //         !TryComp<NtrTaskConsoleComponent>(args.Console, out var consoleComp))
    //     {
    //         return;
    //     }
    //
    //     if (_station.GetOwningStation(args.Console) is not { } station ||
    //         !TryComp<NtrTaskDatabaseComponent>(station, out var db))
    //         return;
    //
    //     foreach (var taskId in documentComp.Tasks)
    //     {
    //         if (!_protoMan.TryIndex(taskId, out NtrTaskPrototype? taskProto))
    //             continue;
    //
    //         if (!TryGetActiveTask(station, taskProto, out var taskData))
    //             continue;
    //
    //         if (TryComp<NtrBankAccountComponent>(station, out var account))
    //             account.Balance += taskProto.Reward;
    //
    //         TryRemoveTask(station, taskData.Value.Id, false);
    //     }
    //
    //     QueueDel(args.Document);
    //     UpdateTaskConsoles();
    // }
//     private bool TryGetActiveTask(EntityUid station, NtrTaskPrototype proto, [NotNullWhen(true)] out NtrTaskData? task)
//     {
//         task = null;
//         return TryComp<NtrTaskDatabaseComponent>(station, out var db) &&
//             (task = db.Tasks.FirstOrDefault(t =>
//                 t.Task == proto.ID && t.IsActive)) != null;
//     }
}
