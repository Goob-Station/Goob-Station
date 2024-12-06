using Content.Server.Storage.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Shared.Storage.EntitySystems;

/// <summary>
/// <see cref="MagnetPickupComponent"/>
/// </summary>
public sealed class MagnetPickupSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;
    [Dependency] private readonly SharedItemSystem _item = default!; // White Dream
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;


    private static readonly TimeSpan ScanDelay = TimeSpan.FromSeconds(1);

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        SubscribeLocalEvent<MagnetPickupComponent, ItemToggledEvent>(OnItemToggled); // White Dream
        SubscribeLocalEvent<MagnetPickupComponent, ExaminedEvent>(OnExamined); // WD EDIT
        SubscribeLocalEvent<MagnetPickupComponent, MapInitEvent>(OnMagnetMapInit);
    }
    //WD EDIT start
    private void OnExamined(Entity<MagnetPickupComponent> entity, ref ExaminedEvent args)
    {
        var onMsg = _itemToggle.IsActivated(entity.Owner)
            ? Loc.GetString("comp-magnet-pickup-examined-on")
            : Loc.GetString("comp-magnet-pickup-examined-off");
        args.PushMarkup(onMsg);
    }

    private void OnItemToggled(Entity<MagnetPickupComponent> entity, ref ItemToggledEvent args)
    {
        _item.SetHeldPrefix(entity.Owner, args.Activated ? "on" : "off");
    }
    //WD EDIT end
    private void OnMagnetMapInit(EntityUid uid, MagnetPickupComponent component, MapInitEvent args)
    {
        component.NextScan = _timing.CurTime;
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<MagnetPickupComponent, StorageComponent, TransformComponent, MetaDataComponent>();
        var currentTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp, out var storage, out var xform, out var meta))
        {
            // WD EDIT START
            if (!TryComp<ItemToggleComponent>(uid, out var toggle))
                continue;

            if (!toggle.Activated)
                continue;
            // WD EDIT END

             if (comp.NextScan > currentTime)
                continue;

            comp.NextScan += ScanDelay;

                        // WD EDIT START. Added ForcePickup.
            if (!comp.ForcePickup && !_inventory.TryGetContainingSlot((uid, xform, meta), out _))
                continue;

            // No space
            if (!_storage.HasSpace((uid, storage)))
                continue;
            //WD EDIT END.
            var parentUid = xform.ParentUid;
            var playedSound = false;
            var finalCoords = xform.Coordinates;
            var moverCoords = _transform.GetMoverCoordinates(uid, xform);

            foreach (var near in _lookup.GetEntitiesInRange(uid, comp.Range, LookupFlags.Dynamic | LookupFlags.Sundries))
            {
                if (_whitelistSystem.IsWhitelistFail(storage.Whitelist, near))
                    continue;

                if (!_physicsQuery.TryGetComponent(near, out var physics) || physics.BodyStatus != BodyStatus.OnGround)
                    continue;

                if (near == parentUid)
                    continue;

                // TODO: Probably move this to storage somewhere when it gets cleaned up
                // TODO: This sucks but you need to fix a lot of stuff to make it better
                // the problem is that stack pickups delete the original entity, which is fine, but due to
                // game state handling we can't show a lerp animation for it.
                var nearXform = Transform(near);
                var nearMap = _transform.GetMapCoordinates(near);
                var nearCoords = EntityCoordinates.FromMap(moverCoords.EntityId, nearMap, _transform, EntityManager);

                if (!_storage.Insert(uid, near, out var stacked, storageComp: storage, playSound: !playedSound))
                    continue;

                // Play pickup animation for either the stack entity or the original entity.
                                _storage.PlayPickupAnimation(stacked ?? near, nearCoords, finalCoords, nearXform.LocalRotation);

                playedSound = true;
            }
        }
    }
}
