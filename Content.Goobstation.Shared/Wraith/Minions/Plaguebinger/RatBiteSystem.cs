using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Minions.Plaguebringer;

public sealed partial class RatBiteSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RatBiteComponent, RatBiteEvent>(OnBite);
    }

    private void OnBite(Entity<RatBiteComponent> ent, ref RatBiteEvent args)
    {
        var uid = ent.Owner;
        var target = args.Target;
        var comp = ent.Comp;

        if (args.Handled || !TryInjectReagents(target, comp.Reagents))
            return;

        _popup.PopupClient(Loc.GetString("wraith-plaguerat-bite-you-message", ("target", target)), uid, uid);
        args.Handled = true;
    }
    private bool TryInjectReagents(EntityUid target, Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> reagents)
    {
        // Build up a solution from the bite's reagents.
        var solution = new Solution();
        foreach (var reagent in reagents)
        {
            solution.AddReagent(reagent.Key, reagent.Value);
        }

        if (!_solution.TryGetInjectableSolution(target, out var targetSolution, out _))
            return false;

        // Try to add the solution to the target's body.
        return _solution.TryAddSolution(targetSolution.Value, solution);
    }
}
