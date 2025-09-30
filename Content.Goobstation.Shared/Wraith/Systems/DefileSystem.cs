using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class DefileSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DefileComponent, DefileEvent>(OnDefile);
    }

    public void OnDefile(Entity<DefileComponent> ent, ref DefileEvent args)
    {
        var uid = ent.Owner;
        var target = args.Target;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        // inject here
        if (TryInjectReagents(target, comp.Reagents))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-defile"), ent.Owner, ent.Owner);
        }

        args.Handled = true;
    }
    private bool TryInjectReagents(EntityUid target, Dictionary<string, FixedPoint2> reagents)
    {
        var solution = new Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Key, reagent.Value);

        if (!_solution.TryGetSolution(target, "drink", out var targetSolution) &&
            !_solution.TryGetSolution(target, "food", out targetSolution))
            return false;

        var solComp = Comp<SolutionComponent>(targetSolution.Value);

        // Ensure capacity is large enough before injecting
        var needed = solComp.Solution.Volume + solution.Volume;
        if (needed > solComp.Solution.MaxVolume)
        {
            solComp.Solution.MaxVolume = needed;
            Dirty(targetSolution.Value, solComp);
        }

        return _solution.TryAddSolution(targetSolution.Value, solution);
    }
}
