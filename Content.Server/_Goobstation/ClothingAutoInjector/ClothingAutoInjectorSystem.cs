using Content.Server.ClothingAutoInjector;
using Content.Shared._Goobstation.Clothing;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.ClothingAutoInjector;

public sealed partial class ClothingAutoInjectorSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingAutoInjectorComponent, ClothingAutoInjectEvent>(OnInject);
    }

    public void OnInject(EntityUid uid, ClothingAutoInjectorComponent component, ClothingAutoInjectEvent action)
    {
        var performer = action.Performer;

        if (!_proto.TryIndex(component.Configuration, out var configuration))
            return;

        TryInjectReagents(performer, configuration.Reagents);
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
