using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;

namespace Content.Goobstation.Shared.Harvestable;

/// <summary>
/// "Click on me to get loot" behavior system
/// </summary>
public sealed class HarvestableSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HarvestableComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<HarvestableComponent, HarvestedDoAfterEvent>(OnHarvestedDoAfter);
    }

    private void OnInteractHand(Entity<HarvestableComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryHarvest(ent, args.User);
    }

    private bool TryHarvest(Entity<HarvestableComponent> ent, EntityUid harvester)
    {
        return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, harvester, ent.Comp.Delay, new HarvestedDoAfterEvent(), ent.Owner, ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            DistanceThreshold = 1.5f,
            RequireCanInteract = true,
            BlockDuplicate = true,
            CancelDuplicate = true,
            NeedHand = true,
        });
    }

    private void OnHarvestedDoAfter(Entity<HarvestableComponent> ent, ref HarvestedDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled)
            return;

        Harvest(ent,args.User);
        args.Handled = true;
    }

    public void Harvest(Entity<HarvestableComponent> ent, EntityUid harvester)
    {
        // Harvest part
        var activeHand =  _handSystem.GetActiveHand(harvester);

        if (ent.Comp.Loot != null)
        {
            var item = PredictedSpawnAtPosition(ent.Comp.Loot, Transform(harvester).Coordinates);
            _handSystem.TryPickup(harvester, item, activeHand, false);
        }

        PredictedDel(ent.Owner);
    }
}
