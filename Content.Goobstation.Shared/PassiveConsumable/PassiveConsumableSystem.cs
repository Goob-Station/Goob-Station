using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.PassiveConsumable;

public sealed class PassiveConsumableSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IngestionSystem _ingestion = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly ReactiveSystem _reactive = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly FlavorProfileSystem _flavor = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassiveConsumableComponent, ClothingGotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<PassiveConsumableComponent, ClothingGotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(Entity<PassiveConsumableComponent> ent, ref ClothingGotEquippedEvent args)
    {
        ent.Comp.Wearer = args.Wearer;
        ent.Comp.NextConsume = _timing.CurTime + ent.Comp.ConsumeInterval;
        Dirty(ent);

        if (!TryComp<EdibleComponent>(ent.Owner, out var edible))
            return;

        if (!_solution.TryGetSolution(ent.Owner, edible.Solution, out _, out var solution))
            return;

        var flavors = _flavor.GetLocalizedFlavorsMessage(args.Wearer, solution);
        _popup.PopupEntity(Loc.GetString("edible-nom", ("food", ent.Owner), ("flavors", flavors)), args.Wearer, args.Wearer);
    }

    private void OnUnequipped(Entity<PassiveConsumableComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ent.Comp.Wearer = null;
        ent.Comp.NextConsume = TimeSpan.Zero;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PassiveConsumableComponent, ClothingComponent, EdibleComponent>();
        List<(EntityUid Uid, EdibleComponent Edible, EntityUid User, bool Delete)>? finished = null;

        while (query.MoveNext(out var uid, out var comp, out var clothing, out var edible))
        {
            if (clothing.InSlotFlag != comp.Slot)
                continue;

            if (comp.NextConsume > _timing.CurTime || comp.NextConsume == TimeSpan.Zero)
                continue;

            if (TryConsume((uid, comp), edible, out var shouldDelete))
            {
                finished ??= [];
                finished.Add((uid, edible, comp.Wearer!.Value, shouldDelete));
            }

            comp.NextConsume = _timing.CurTime + comp.ConsumeInterval;
        }

        if (finished == null)
            return;

        foreach (var (uid, edible, user, delete) in finished)
        {
            _ingestion.SpawnTrash((uid, edible), user);

            if (delete)
                QueueDel(uid);
        }
    }

    /// <returns>True if the item's solution is now empty.</returns>
    private bool TryConsume(Entity<PassiveConsumableComponent> ent, EdibleComponent edible, out bool shouldDelete)
    {
        shouldDelete = false;

        if (ent.Comp.Wearer is not { } wearer
            || !TryComp<BodyComponent>(wearer, out var body)
            || !_body.TryGetBodyOrganEntityComps<StomachComponent>((wearer, body), out var stomachs)
            || !_solution.TryGetSolution(ent.Owner, edible.Solution, out var soln, out var solution))
            return false;

        var transferAmount = FixedPoint2.Min(ent.Comp.Amount, solution.Volume);
        var split = _solution.SplitSolution(soln.Value, transferAmount);

        var highestAvailable = FixedPoint2.Zero;
        StomachComponent? bestStomach = null;
        EntityUid? bestStomachUid = null;

        foreach (var organ in stomachs)
        {
            if (!_stomach.CanTransferSolution(organ.Owner, split, organ.Comp1)
                || !_solution.ResolveSolution(organ.Owner, StomachSystem.DefaultSolutionName, ref organ.Comp1.Solution, out var stomachSol)
                || stomachSol.AvailableVolume <= highestAvailable)
                continue;

            bestStomach = organ.Comp1;
            bestStomachUid = organ.Owner;
            highestAvailable = stomachSol.AvailableVolume;
        }

        if (bestStomachUid == null)
        {
            _solution.TryAddSolution(soln.Value, split);
            return false;
        }

        _reactive.DoEntityReaction(wearer, solution, ReactionMethod.Ingestion);
        _stomach.TryTransferSolution(bestStomachUid.Value, split, bestStomach);

        if (soln.Value.Comp.Solution.Volume > FixedPoint2.Zero)
            return false;

        ent.Comp.NextConsume = TimeSpan.Zero;
        shouldDelete = ent.Comp.DeleteOnEmpty;
        return true;
    }
}
