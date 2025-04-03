using Content.Shared.Lock;
using Content.Shared.Projectiles;
using Content.Shared.Storage.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Shared.Storage.EntitySystems;

internal sealed class StoreOnCollideSystem : EntitySystem
{
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly LockSystem _lock = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoreOnCollideComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<StoreOnCollideComponent, StorageAfterOpenEvent>(AfterOpen);
        // TODO: Add support to stop colliding after throw, wands will need a WandComp

        SubscribeLocalEvent<StoreOnCollideComponent, TimedDespawnEvent>(OnTimedDespawn); // Goobstation
        SubscribeLocalEvent<StoreOnCollideComponent, LockToggledEvent>(OnLockToggle); // Goobstation
        SubscribeLocalEvent<StoreOnCollideComponent, PhysicsSleepEvent>(OnSleep); // Goobstation
    }

    // Goobstation start

    private void OnSleep(Entity<StoreOnCollideComponent> ent, ref PhysicsSleepEvent args)
    {
        var comp = ent.Comp;

        if (comp is { DisableOnSleep: true, Disabled: false })
            Disable(ent);
    }

    private void OnLockToggle(Entity<StoreOnCollideComponent> ent, ref LockToggledEvent args)
    {
        if (args.Locked)
            return;

        var comp = ent.Comp;

        if (comp is { DisableWhenFirstOpened: true, Disabled: false })
            Disable(ent);
    }

    private void OnTimedDespawn(Entity<StoreOnCollideComponent> ent, ref TimedDespawnEvent args)
    {
        _storage.OpenStorage(ent);
    }

    private void Disable(Entity<StoreOnCollideComponent> ent)
    {
        ent.Comp.Disabled = true;
        Dirty(ent);
    }
    // Goobstation end

    // We use Collide instead of Projectile to support different types of interactions
    private void OnCollide(Entity<StoreOnCollideComponent> ent, ref StartCollideEvent args)
    {
        TryStoreTarget(ent, args.OtherEntity);

        TryLockStorage(ent);

        // Goobstation start
        if (!TryComp(ent.Owner, out ProjectileComponent? projectile))
            return;

        ent.Comp.IgnoredEntity = projectile.Shooter;
        projectile.IgnoreShooter = false;
        Entity<ProjectileComponent, StoreOnCollideComponent> toDirty = (ent.Owner, projectile, ent.Comp);
        Dirty(toDirty);
        // Goobstation end
    }

    private void AfterOpen(Entity<StoreOnCollideComponent> ent, ref StorageAfterOpenEvent args)
    {
        var comp = ent.Comp;

        if (comp is { DisableWhenFirstOpened: true, Disabled: false })
            Disable(ent); // Goob edit
    }

    private void TryStoreTarget(Entity<StoreOnCollideComponent> ent, EntityUid target)
    {
        var storageEnt = ent.Owner;
        var comp = ent.Comp;

        if (_netMan.IsClient || _gameTiming.ApplyingState)
            return;

        if (target == comp.IgnoredEntity) // Goobstation
            return;

        if (ent.Comp.Disabled || storageEnt == target || Transform(target).Anchored || _storage.IsOpen(storageEnt) || _whitelist.IsWhitelistFail(comp.Whitelist, target))
            return;

        _storage.Insert(target, storageEnt);

    }

    private void TryLockStorage(Entity<StoreOnCollideComponent> ent)
    {
        var storageEnt = ent.Owner;
        var comp = ent.Comp;

        if (_netMan.IsClient || _gameTiming.ApplyingState)
            return;

        if (ent.Comp.Disabled)
            return;

        if (comp.LockOnCollide && !_lock.IsLocked(storageEnt))
            _lock.Lock(storageEnt, storageEnt);
    }
}
