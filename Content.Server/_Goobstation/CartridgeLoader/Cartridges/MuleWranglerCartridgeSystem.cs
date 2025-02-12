using Content.Server.CartridgeLoader;
using Content.Shared._Goobstation.CartridgeLoader.Cartridges;
using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.CartridgeLoader;

namespace Content.Server._Goobstation.CartridgeLoader.Cartridges;

public sealed class MuleWranglerCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem? _cartridgeLoaderSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

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
        switch (msg.Type)
        {
            case MuleWranglerMessageType.Transport:
                if(!TryComp<MuleComponent>(uid, out var comp))
                    return;
                comp.CurrentOrder = MuleOrderType.Transport;
                break;
        }

        UpdateUiState(ent, GetEntity(args.LoaderUid));
    }

    private void OnUiReady(Entity<MuleWranglerCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUiState(ent, args.Loader);
    }

    private void UpdateUiState(Entity<MuleWranglerCartridgeComponent> ent, EntityUid loaderUid)
    {
        var query = _entityManager.EntityQueryEnumerator<MuleComponent>();
        var queryBeacon = _entityManager.EntityQueryEnumerator<MuleDropOffComponent>();
        List<NetEntity> list = new();
        List<NetEntity> listBeacon = new();

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

        var state = new MuleWranglerUiState(list,listBeacon);
        _cartridgeLoaderSystem?.UpdateCartridgeUiState(loaderUid, state);
    }
}
