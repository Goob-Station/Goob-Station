using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Store.Systems;
using Content.Server.Traitor.Uplink;
using Content.Shared._Goobstation.Traitor.Contracts;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Objectives.Systems;
using Content.Shared.Store.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Traitor.Contracts;

public sealed class ContractSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private int _nextContractId = 1;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContractsComponent, ContractAcceptMessage>(OnContractAccept);
        SubscribeLocalEvent<ContractsComponent, ContractAbandonMessage>(OnContractAbandon);
        SubscribeLocalEvent<ContractsComponent, ContractClaimRewardMessage>(OnContractClaimReward);
        SubscribeLocalEvent<ContractsComponent, ContractRequestUpdateMessage>(OnContractRequestUpdate);
        SubscribeLocalEvent<OpenContractsActionEvent>(OnOpenContractsAction);
    }

    private void OnOpenContractsAction(OpenContractsActionEvent args)
    {
        if (args.Handled)
            return;

        var performer = args.Performer;

        EntityUid? contractsEntity = null;

        if (_inventory.TryGetContainerSlotEnumerator(performer, out var enumerator))
        {
            while (enumerator.MoveNext(out var slot))
            {
                if (slot.ContainedEntity is { } item && HasComp<ContractsComponent>(item))
                {
                    contractsEntity = item;
                    break;
                }
            }
        }

        if (contractsEntity == null)
        {
            foreach (var held in _hands.EnumerateHeld(performer))
            {
                if (HasComp<ContractsComponent>(held))
                {
                    contractsEntity = held;
                    break;
                }
            }
        }

        if (contractsEntity == null)
        {
            _popup.PopupEntity(Loc.GetString("contract-no-uplink"), performer, performer);
            return;
        }

        _ui.TryToggleUi(contractsEntity.Value, ContractUiKey.Key, performer);
        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ContractsComponent>();
        while (query.MoveNext(out var uid, out var contracts))
        {
            UpdateContractProgress(uid, contracts);
        }
    }

    private void OnContractAccept(EntityUid uid, ContractsComponent component, ContractAcceptMessage args)
    {
        if (!_proto.TryIndex(args.ContractId, out var proto))
            return;

        if (component.ActiveContracts.Count >= component.MaxActiveContracts)
            return;

        if (!proto.Repeatable && component.ActiveContracts.Any(c => c.PrototypeId == proto.ID))
            return;

        EntityUid? mindId = null;
        MindComponent? mindComp = null;

        if (TryComp<MindComponent>(uid, out var mind))
        {
            mindId = uid;
            mindComp = mind;
        }
        else if (TryComp<StoreComponent>(uid, out var store) && store.AccountOwner != null)
        {
            mindId = store.AccountOwner;
            if (!TryComp(mindId, out mindComp))
                return;
        }

        if (mindId == null || mindComp == null)
            return;

        var objectiveUid = _objectives.TryCreateObjective(mindId.Value, mindComp, proto.ObjectivePrototype);
        if (objectiveUid == null)
            return;

        _mind.AddObjective(mindId.Value, mindComp, objectiveUid.Value);

        var info = _objectives.GetInfo(objectiveUid.Value, mindId.Value, mindComp);

        var contractData = new ContractData
        {
            ContractId = _nextContractId++,
            PrototypeId = proto.ID,
            ObjectiveEntity = GetNetEntity(objectiveUid),
            Progress = 0f,
            AcceptedTime = _timing.CurTime,
            Status = ContractStatus.Active,
            Title = info?.Title ?? Loc.GetString(proto.Name),
            Description = info?.Description ?? Loc.GetString(proto.Description),
            Reward = proto.Reward
        };

        component.ActiveContracts.Add(contractData);
        Dirty(uid, component);

        UpdateUi(uid, component);
    }

    private void OnContractAbandon(EntityUid uid, ContractsComponent component, ContractAbandonMessage args)
    {
        var contract = component.ActiveContracts.FirstOrDefault(c => c.ContractId == args.ContractInstanceId);
        if (contract == null)
            return;

        contract.Status = ContractStatus.Abandoned;
        component.ActiveContracts.Remove(contract);
        component.FailedContracts.Add(contract);

        if (contract.ObjectiveEntity != null && TryGetMindForContracts(uid, out var mindId, out var mindComp))
        {
            var objectiveUid = GetEntity(contract.ObjectiveEntity.Value);
            var index = mindComp.Objectives.IndexOf(objectiveUid);
            if (index >= 0)
                _mind.TryRemoveObjective(mindId, mindComp, index);
        }

        Dirty(uid, component);
        UpdateUi(uid, component);
    }

    private void OnContractClaimReward(EntityUid uid, ContractsComponent component, ContractClaimRewardMessage args)
    {
        var contract = component.ActiveContracts.FirstOrDefault(c => c.ContractId == args.ContractInstanceId && c.Status == ContractStatus.Completed);
        if (contract == null)
            return;

        var reward = contract.Reward;
        if (_proto.TryIndex(contract.PrototypeId, out var proto) && proto.PartialReward)
        {
            reward = (int)(reward * contract.Progress);
        }

        if (component.LinkedUplink != null && TryComp<StoreComponent>(component.LinkedUplink, out var store))
        {
            var currency = new Dictionary<string, FixedPoint2>
            {
                { UplinkSystem.TelecrystalCurrencyPrototype, reward }
            };
            _store.TryAddCurrency(currency, component.LinkedUplink.Value, store);
        }

        component.ActiveContracts.Remove(contract);
        component.CompletedContracts.Add(contract);
        component.TotalEarned += reward;

        Dirty(uid, component);
        UpdateUi(uid, component);
    }

    private void OnContractRequestUpdate(EntityUid uid, ContractsComponent component, ContractRequestUpdateMessage args)
    {
        UpdateUi(uid, component);
    }

    private void UpdateContractProgress(EntityUid uid, ContractsComponent component)
    {
        if (!TryGetMindForContracts(uid, out var mindId, out var mindComp))
            return;

        var changed = false;

        foreach (var contract in component.ActiveContracts.ToList())
        {
            if (contract.Status != ContractStatus.Active || contract.ObjectiveEntity == null)
                continue;

            var objectiveUid = GetEntity(contract.ObjectiveEntity.Value);

            var progress = _objectives.GetProgress(objectiveUid, (mindId, mindComp));
            if (progress == null)
                continue;

            var oldProgress = contract.Progress;
            contract.Progress = progress.Value;

            if (progress >= 0.999f && contract.Status == ContractStatus.Active)
            {
                contract.Status = ContractStatus.Completed;
                contract.CompletedTime = _timing.CurTime;
                changed = true;
            }
            else if (Math.Abs(oldProgress - contract.Progress) > 0.01f)
            {
                changed = true;
            }
        }

        if (changed)
        {
            Dirty(uid, component);
            UpdateUi(uid, component);
        }
    }

    public List<ContractData> GetAvailableContracts(EntityUid uid, ContractsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return new List<ContractData>();

        var available = new List<ContractData>();

        foreach (var proto in _proto.EnumeratePrototypes<ContractPrototype>())
        {
            if (!proto.Repeatable && component.ActiveContracts.Any(c => c.PrototypeId == proto.ID))
                continue;

            if (!proto.Repeatable && component.CompletedContracts.Any(c => c.PrototypeId == proto.ID))
                continue;

            available.Add(new ContractData
            {
                ContractId = 0,
                PrototypeId = proto.ID,
                Status = ContractStatus.Available,
                Title = Loc.GetString(proto.Name),
                Description = Loc.GetString(proto.Description),
                Reward = proto.Reward
            });
        }

        return available;
    }

    public void UpdateUi(EntityUid uid, ContractsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var state = new ContractMenuState(
            GetAvailableContracts(uid, component),
            component.ActiveContracts,
            component.CompletedContracts,
            component.TotalEarned,
            component.MaxActiveContracts
        );

        _ui.SetUiState(uid, ContractUiKey.Key, state);
    }

    private bool TryGetMindForContracts(EntityUid uid, out EntityUid mindId, [NotNullWhen(true)] out MindComponent? mindComp)
    {
        mindId = default;
        mindComp = null;

        if (TryComp<MindComponent>(uid, out var mind))
        {
            mindId = uid;
            mindComp = mind;
            return true;
        }

        if (TryComp<StoreComponent>(uid, out var store) && store.AccountOwner != null)
        {
            mindId = store.AccountOwner.Value;
            return TryComp(mindId, out mindComp);
        }

        return false;
    }

    public void LinkUplink(EntityUid contractsEntity, EntityUid uplinkEntity, ContractsComponent? component = null)
    {
        if (!Resolve(contractsEntity, ref component))
            return;

        component.LinkedUplink = uplinkEntity;
        Dirty(contractsEntity, component);
    }
}
