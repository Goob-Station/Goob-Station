using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.NTR;
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
// goidacore inside
namespace Content.Goobstation.Server.NTR;

public sealed partial class NtrTaskSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSys = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
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

    private readonly ProtoId<NameIdentifierGroupPrototype> _nameIdentifierGroup = "Task";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NtrAccountClientComponent, NtrAccountBalanceUpdatedEvent>(OnBalanceUpdated);
        SubscribeLocalEvent<NtrTaskProviderComponent, BoundUIOpenedEvent>(OnOpened);
        SubscribeLocalEvent<NtrTaskProviderComponent, TaskPrintLabelMessage>(OnPrintLabelMessage);
        SubscribeLocalEvent<NtrTaskProviderComponent, TaskSkipMessage>(OnTaskSkipMessage);
        SubscribeLocalEvent<NtrTaskDatabaseComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<NtrTaskProviderComponent, TaskCompletedEvent>(OnTaskCompleted);
    }
    private void OnBalanceUpdated(EntityUid uid, NtrAccountClientComponent clientComp, ref NtrAccountBalanceUpdatedEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var storeComp))
            return;
        var newBalance = new Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2>
        {
            { "NTLoyaltyPoint", FixedPoint2.New(args.Balance) }
        };
        storeComp.Balance = newBalance;
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

    private void OnTaskCompleted(EntityUid uid, NtrTaskProviderComponent component, TaskCompletedEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<NtrTaskDatabaseComponent>(station, out var db))
            return;
        if (!TryGetTaskId(station, args.Task, out var taskData))
            return;
        if (!TryComp<StationNtrAccountComponent>(station, out var ntrAccount))
            return;

        ntrAccount.Balance += args.Task.Reward;
        var query = EntityQueryEnumerator<NtrAccountClientComponent>();
        var ev = new NtrAccountBalanceUpdatedEvent(uid, ntrAccount.Balance);
        while (query.MoveNext(out var client, out var comp))
        {
            comp.Balance = ntrAccount.Balance;
            Dirty(client, comp);
            RaiseLocalEvent(client, ref ev);
        }

        if (!TryRemoveTask(station, taskData, false))
            return;

        FillTasksDatabase(station);
        var untilNextSkip = db.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(uid, NtrTaskUiKey.Key,
            new NtrTaskProviderState(db.Tasks, db.History, untilNextSkip));
        _audio.PlayPvs(component.SkipSound, uid);
    }

    private void OnPrintLabelMessage(EntityUid uid, NtrTaskProviderComponent component, TaskPrintLabelMessage args)
    {
        if (_timing.CurTime < component.NextPrintTime)
            return;

        if (_station.GetOwningStation(uid) is not { } station)
            return;

        if (!TryGetTaskFromId(station, args.TaskId, out var task))
            return;

        if (!_protoMan.TryIndex(task.Value.Task, out var ntrPrototype))
            return;

        if (ntrPrototype.Entries.Any(e => e.IsEvent))
        {
            var ev = new TaskCompletedEvent(ntrPrototype);
            RaiseLocalEvent(uid, ev);

            component.NextPrintTime = _timing.CurTime + component.PrintDelay;
            _audio.PlayPvs(component.SkipSound, uid);
            return;
        }
        Spawn(ntrPrototype.Proto, Transform(uid).Coordinates);
        component.NextPrintTime = _timing.CurTime + component.PrintDelay;
        _audio.PlayPvs(component.PrintSound, uid);
    }


    /// <summary>
    /// При открытии интерфейса получаем станцию через GetOwningStation, из станции вытаскиваем компонент базы данных задач НТР
    /// Вычисляем время до скипа задачи, поднимаем метод UpdateState в NtrTaskBoundUserInterface чтобы данные интерфейса изменились
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
    private void OnOpened(EntityUid uid, NtrTaskProviderComponent component, BoundUIOpenedEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<NtrTaskDatabaseComponent>(station, out var taskDb))
            return;

        var untilNextSkip = taskDb.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(uid, NtrTaskUiKey.Key, new NtrTaskProviderState(taskDb.Tasks, taskDb.History, untilNextSkip));
    }

    /// <summary>
    /// Поднимается при нажатии кнопки Skip, получаем станцию, базу данных, задачу с которой работаем.
    /// AccessReader не трогал, но скорее всего просто установить у прототипа уровень доступа НТРа
    /// Удаляем задачу, заполняем базу данных методом FillTaskDatabase новой случайной задачей.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
    private void OnTaskSkipMessage(EntityUid uid, NtrTaskProviderComponent component, TaskSkipMessage args)
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
        _uiSystem.SetUiState(uid, NtrTaskUiKey.Key, new NtrTaskProviderState(db.Tasks, db.History, untilNextSkip));
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
        FillTasksDatabase(uid, component);
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

        // todo: consider making the cargo bounties weighted.
        var allTasks = _protoMan.EnumeratePrototypes<NtrTaskPrototype>().ToList();
        var filteredTasks = new List<NtrTaskPrototype>();
        foreach (var proto in allTasks)
        {
            if (component.Tasks.Any(b => b.Task == proto.ID))
                continue;
            filteredTasks.Add(proto);
        }

        var pool = filteredTasks.Count == 0 ? allTasks : filteredTasks;
        var task = _random.Pick(pool);
        return TryAddTask(uid, task, component);
    }

    /// <summary>
    /// ХЗ для чего дополнительные такие методы, увынск
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="taskId"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    [PublicAPI]
    public bool TryAddTask(EntityUid uid, string taskId, NtrTaskDatabaseComponent? component = null)
    {
        if (!_protoMan.TryIndex<NtrTaskPrototype>(taskId, out var task))
        {
            return false;
        }

        return TryAddTask(uid, task, component);
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
    public void UpdateTaskConsoles()
    {
        var query = EntityQueryEnumerator<NtrTaskProviderComponent, UserInterfaceComponent>();
        while (query.MoveNext(out var uid, out _, out var ui))
        {
            if (_station.GetOwningStation(uid) is not { } station ||
                !TryComp<NtrTaskDatabaseComponent>(station, out var db))
            {
                continue;
            }

            var untilNextSkip = db.NextSkipTime - _timing.CurTime;
            _uiSystem.SetUiState((uid, ui), NtrTaskUiKey.Key, new NtrTaskProviderState(db.Tasks, db.History, untilNextSkip));
        }
    }
}
