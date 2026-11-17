using Content.Goobstation.Shared.Bingle;
using Content.Goobstation.Shared.Bongle;
using Content.Shared.Actions.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Rejuvenate;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Bongle;

public sealed partial class BongleSystem : EntitySystem
{
    [Dependency] private MobStateSystem _mob = default!;
    [Dependency] private SharedContainerSystem _container = default!;

    private readonly List<Entity<BongleComponent>> _bongles = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BongleComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BongleComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;

        if (!comp.Prime || comp.HillProto is not { } hillProto)
            return;

        var coords = Transform(uid).Coordinates;

        if (!coords.IsValid(EntityManager) || Transform(uid).MapID == MapId.Nullspace)
            return;

        PredictedSpawnAtPosition(hillProto, coords);
    }

    public void UpgradeBongles()
    {
        CollectBongles();

        foreach (var (uid, _) in _bongles)
        {
            if (TryComp<BingleComponent>(uid, out var bingle) && bingle.Upgraded)
                continue;

            // the poly that bingles and bongles do replaces the entity so we kinda have to empty their stomach otherwise...issues
            if (TryComp<SwallowerComponent>(uid, out var swallower))
            {
                _container.EmptyContainer(swallower.Stomach);
            }

            RaiseLocalEvent(uid, new BingleUpgradeActionEvent());
        }
    }

    // heal bongle, let them chew people and give them devour wall action
    public void AscendBongles()
    {
        CollectBongles();

        foreach (var (uid, comp) in _bongles)
        {
            RaiseLocalEvent(uid, new RejuvenateEvent());

            if (TryComp<SwallowerComponent>(uid, out var swallower))
            {
                swallower.ChewBlacklist = null;
                Dirty(uid, swallower);
            }

            EntityManager.AddComponents(uid, comp.AscendComponents);
        }
    }

    private void CollectBongles()
    {
        _bongles.Clear();

        var query = EntityQueryEnumerator<BongleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_mob.IsAlive(uid) || _container.IsEntityInContainer(uid))
                continue;

            _bongles.Add((uid, comp));
        }
    }
}
