using Content.Server.CartridgeLoader;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared._Goobstation.CartridgeLoader.Cartridges;
using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.NPC.Systems;

namespace Content.Server._Goobstation.CartridgeLoader.Cartridges;

public sealed class MuleWranglerCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem? _cartridgeLoaderSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly NPCSystem _npc = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MuleWranglerCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<MuleWranglerCartridgeComponent, CartridgeMessageEvent>(OnMessage);
    }

    private void OnMessage(Entity<MuleWranglerCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not MuleWranglerUiMessageEvent msg)
            return;

        var uid = GetEntity(msg.MuleEntity);
        if(!TryComp<MuleComponent>(uid, out var muleComponent))
            return;
        switch (msg.Type)
        {
            case MuleWranglerMessageType.Transport:
                muleComponent.CurrentOrder = MuleOrderType.Transport;
                UpdateMuleBlackboard(uid, MuleOrderType.Transport);
                break;
            case MuleWranglerMessageType.SetDestination:
                if (msg.DropOffEntity is null)
                    return;
                if (!TryGetEntity(msg.DropOffEntity, out var entity))
                    return;
                if (entity is not { } realEntity)
                    return;
                ent.Comp.SelectedBeacon = (NetEntity) msg.DropOffEntity;
                muleComponent.CurrentTarget = realEntity;
                break;
            case MuleWranglerMessageType.Return:
                break;
            case MuleWranglerMessageType.SetMule:
                ent.Comp.SelectedMule = msg.MuleEntity;
                break;
            case MuleWranglerMessageType.Unload:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        UpdateUiState(ent, GetEntity(args.LoaderUid));
    }

    private void OnUiReady(Entity<MuleWranglerCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUiState(ent, args.Loader);
    }

    public void UpdateMuleBlackboard(EntityUid uid, MuleOrderType orderType)
    {
        if (!TryComp<HTNComponent>(uid, out var htn))
            return;

        if (htn.Plan != null)
            _htn.ShutdownPlan(htn);

        _npc.SetBlackboard(uid, NPCBlackboard.CurrentOrders, orderType);
        _htn.Replan(htn);
    }

    private void UpdateUiState(Entity<MuleWranglerCartridgeComponent> ent, EntityUid loaderUid)
    {
        var query = _entityManager.EntityQueryEnumerator<MuleComponent>();
        var queryBeacon = _entityManager.EntityQueryEnumerator<MuleDropOffComponent>();
        List<NetEntity> list = new();
        List<NetEntity> listBeacon = new();
        var selectedMule = NetEntity.Invalid;
        var selectedBeacon = NetEntity.Invalid;

        while(query.MoveNext(out var uid,  out var _))
        {
            Logger.Debug(uid.ToString());
            var netEntity = GetNetEntity(uid);
            if(netEntity.Id is not 0)
                list.Add(netEntity);
        }
        while(queryBeacon.MoveNext(out var uid,  out var _))
        {
            Logger.Debug(uid.ToString());
            var netEntity = GetNetEntity(uid);
            if(netEntity.Id is not 0)
                listBeacon.Add(netEntity);
        }

        if (ent.Comp.SelectedMule.Id is not 0)
        {
            selectedMule = ent.Comp.SelectedMule;
        }

        if (ent.Comp.SelectedBeacon.Id is not 0)
        {
            selectedBeacon = ent.Comp.SelectedMule;
        }

        var state = new MuleWranglerUiState(list,listBeacon, selectedMule, selectedBeacon);
        _cartridgeLoaderSystem?.UpdateCartridgeUiState(loaderUid, state);
    }
}
