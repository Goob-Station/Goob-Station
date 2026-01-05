using Content.Server.Popups;
using Content.Shared.Clothing.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Nutrition.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry;
using Content.Shared.Clothing;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Lollypop;

public sealed class LollypopSystem : EntitySystem
{

    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly FoodSystem _food = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly ReactiveSystem _reaction = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly FlavorProfileSystem _flavorProfile = default!;
    [Dependency] private readonly IGameTiming _time = default!;



    public override void Initialize()
    {
        SubscribeLocalEvent<LollypopComponent,ClothingGotEquippedEvent>(OnEquipt);
        SubscribeLocalEvent<LollypopComponent,ClothingGotUnequippedEvent>(OnUnequipt);
    }

    public override void Update(float frameTime)
    {
        var query = EntityManager.EntityQueryEnumerator<LollypopComponent, ClothingComponent, FoodComponent>();

        while (query.MoveNext(out var queryUid, out var lollypop, out var clothing, out var food))
        {
            if (clothing.InSlotFlag != lollypop.CheckSlot)
                continue;

            if(lollypop.NextBite > _time.CurTime && lollypop.NextBite != TimeSpan.Zero)
                continue;

            Eat((queryUid,lollypop),food);
            lollypop.NextBite = _time.CurTime + lollypop.BiteInterval;
        }
    }

    private void OnEquipt(Entity<LollypopComponent> ent, ref ClothingGotEquippedEvent args)
    {
        ent.Comp.HeldBy = args.Wearer;
        ent.Comp.NextBite = _time.CurTime + ent.Comp.BiteInterval;

        // add popup of taste
        if (!TryComp<FoodComponent>(ent.Owner, out var food))
            return;
        if (!_solutionContainer.TryGetSolution(ent.Owner, food.Solution, out var soln, out _))
            return;

        var flavors = _flavorProfile.GetLocalizedFlavorsMessage(args.Wearer, soln.Value.Comp.Solution);
        _popup.PopupEntity(Loc.GetString(food.EatMessage, ("food", ent.Owner), ("flavors", flavors)), args.Wearer,args.Wearer);
    }

    private void OnUnequipt(Entity<LollypopComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ent.Comp.HeldBy = null;
        ent.Comp.NextBite = TimeSpan.Zero;
    }

    private void Eat(Entity<LollypopComponent> ent, FoodComponent food)
    {
        if(ent.Comp.HeldBy == null)
            return;

        if (!TryComp<BodyComponent>(ent.Comp.HeldBy, out var body))
            return;
        if (!_body.TryGetBodyOrganEntityComps<StomachComponent>((ent.Comp.HeldBy.Value, body), out var stomachs))
            return;
        if (!_solutionContainer.TryGetSolution(ent.Owner, food.Solution, out var soln, out var solution))
            return;

        var transferAmount = FixedPoint2.Min( ent.Comp.Ammount, solution.Volume);

        var split = _solutionContainer.SplitSolution(soln.Value, transferAmount);

        // Get the stomach with the highest available solution volume
        var highestAvailable = FixedPoint2.Zero;
        Entity<StomachComponent>? stomachToUse = null;
        foreach (var entity in stomachs)
        {
            var owner = entity.Owner;
            if (!_stomach.CanTransferSolution(owner, split, entity.Comp1))
                continue;

            if (!_solutionContainer.ResolveSolution(owner, StomachSystem.DefaultSolutionName, ref entity.Comp1.Solution, out var stomachSol))
                continue;

            if (stomachSol.AvailableVolume <= highestAvailable)
                continue;

            stomachToUse = entity;
            highestAvailable = stomachSol.AvailableVolume;
        }

        if (stomachToUse == null)
        {
            _solutionContainer.TryAddSolution(soln.Value, split);
            return;
        }

        _reaction.DoEntityReaction(ent.Comp.HeldBy.Value, solution, ReactionMethod.Ingestion);
        _stomach.TryTransferSolution(stomachToUse!.Value.Owner, split, stomachToUse);

        if (soln.Value.Comp.Solution.Volume > FixedPoint2.Zero )
            return; // end if there is solution left

        if (ent.Comp.DeleteOnEmpty)
            _food.DeleteAndSpawnTrash(food,ent.Owner,ent.Comp.HeldBy.Value);

        ent.Comp.NextBite  = TimeSpan.Zero; // lollypop is empty stop checking
    }
}
