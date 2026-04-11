using Content.Server.Popups;
using Content.Shared.Clothing.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry;
using Content.Shared.Clothing;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;

namespace Content.Goobstation.Server.Lollypop;

public sealed class LollypopSystem : EntitySystem
{

    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IngestionSystem _ingestion = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly ReactiveSystem _reaction = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly FlavorProfileSystem _flavorProfile = default!;

    private float _updateCooldown = 3f;
    private float _nextUpdate;

    public override void Initialize()
    {
        SubscribeLocalEvent<LollypopComponent,ClothingGotEquippedEvent>(OnEquipt);
        SubscribeLocalEvent<LollypopComponent,ClothingGotUnequippedEvent>(OnUnequipt);
    }

    public override void Update(float frameTime)
    {
        _nextUpdate += frameTime;

        if (_nextUpdate < _updateCooldown)
            return;

        var query = EntityManager.EntityQueryEnumerator<LollypopComponent, ClothingComponent, EdibleComponent>();
        while (query.MoveNext(out var queryUid, out var lollypop, out var clothing, out var edible))
        {
            if (clothing.InSlotFlag != lollypop.CheckSlot)
                continue;

            Eat((queryUid, lollypop), edible);
        }
        _nextUpdate -= _updateCooldown;
    }

    private void OnEquipt(Entity<LollypopComponent> ent, ref ClothingGotEquippedEvent args)
    {
        ent.Comp.HeldBy = args.Wearer;

        // add popup of taste
        if (!TryComp<EdibleComponent>(ent.Owner, out var edible))
            return;
        if (!_solutionContainer.TryGetSolution(ent.Owner, edible.Solution, out var soln, out _))
            return;

        var flavors = _flavorProfile.GetLocalizedFlavorsMessage(args.Wearer, soln.Value.Comp.Solution);
        _popup.PopupEntity(Loc.GetString("edible-nom", ("food", ent.Owner), ("flavors", flavors)), args.Wearer,args.Wearer);
    }

    private void OnUnequipt(Entity<LollypopComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ent.Comp.HeldBy = null;
    }

    private void Eat(Entity<LollypopComponent> ent, EdibleComponent edible)
    {
        if (ent.Comp.HeldBy == null ||
            !_body.TryGetBodyOrganEntityComps<StomachComponent>(ent.Comp.HeldBy.Value, out var stomachs))
            return;

        if (!_solutionContainer.TryGetSolution(ent.Owner, edible.Solution, out var soln, out var solution))
            return;

        var transferAmount = FixedPoint2.Min(ent.Comp.Amount, solution.Volume);
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
        _stomach.TryTransferSolution(stomachToUse.Value.Owner, split, stomachToUse);

        if (soln.Value.Comp.Solution.Volume > FixedPoint2.Zero )
            return;

        if (ent.Comp.DeleteOnEmpty)
        {
            QueueDel(ent);
            _ingestion.SpawnTrash((ent.Owner, edible), ent.Comp.HeldBy.Value);
        }
    }
}
