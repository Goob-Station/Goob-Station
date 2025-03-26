using Content.Shared._Goobstation.Clothing;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Clothing;

public sealed partial class ClothingAutoinjectorSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingAutoInjectComponent, ActionActivateAutoInjectorEvent>(OnInjectorActivated);
    }

    public void OnInjectorActivated(EntityUid uid, ClothingAutoInjectComponent component, ActionActivateAutoInjectorEvent args)
    {
        if (!_proto.TryIndex(component.Proto, out var proto))
            return;

        TryInjectReagents(args.Performer, proto.Reagents);
    }

    public bool TryInjectReagents(EntityUid uid, Dictionary<string, FixedPoint2> reagents)
    {
        var solution = new Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Key, reagent.Value);

        if (!_solution.TryGetInjectableSolution(uid, out var targetSolution, out var _))
            return false;

        if (!_solution.TryAddSolution(targetSolution.Value, solution))
            return false;

        return true;
    }
}
