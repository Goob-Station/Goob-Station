using Content.Server.CartridgeLoader;
using Content.Shared._Goobstation.CartridgeLoader.Cartridges;
using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.CartridgeLoader;

namespace Content.Server._Goobstation.CartridgeLoader.Cartridges;

public sealed partial class MuleWranglerCartridgeSystem : EntitySystem
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
        UpdateUiState(ent, GetEntity(args.LoaderUid));
    }

    private void OnUiReady(Entity<MuleWranglerCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUiState(ent, args.Loader);
    }

    private void UpdateUiState(Entity<MuleWranglerCartridgeComponent> ent, EntityUid loaderUid)
    {
        var query = _entityManager.EntityQueryEnumerator<MuleComponent>();
        List<NetEntity> list = new();
        while(query.MoveNext(out var uid,  out var _))
        {
            Logger.Debug(uid.ToString());
            var netEntity = GetNetEntity(uid);
            if(netEntity.Id is not 0)
                list.Add(netEntity);
        }

        var state = new MuleWranglerUiState(list);
        _cartridgeLoaderSystem?.UpdateCartridgeUiState(loaderUid, state);
    }
}
