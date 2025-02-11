using Content.Server.CartridgeLoader;
using Content.Server.CartridgeLoader.Cartridges;
using Content.Shared._Goobstation.CartridgeLoader.Cartridges;
using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.CartridgeLoader.Cartridges;

public sealed partial class MuleWranglerCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem? _cartridgeLoaderSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MuleWranglerCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
    }

    private void OnUiReady(Entity<MuleWranglerCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUiState(ent, args.Loader);
    }

    private void UpdateUiState(Entity<MuleWranglerCartridgeComponent> ent, EntityUid loaderUid)
    {
        var query = _entityManager.EntityQueryEnumerator<MuleComponent>();
        List<NetEntity> list = new();
        while(query.MoveNext(out var uid,  out var comp))
        {
            var netEntity = GetNetEntity(uid);
            if(netEntity.Id is not 0)
                list.Add(netEntity);
        }

        var state = new MuleWranglerUiState(list);
        _cartridgeLoaderSystem?.UpdateCartridgeUiState(loaderUid, state);
    }
}
