using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Goobstation.Shared.Chemistry;

public sealed partial class InjectOnSpawnSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly ReactiveSystem _reactive = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InjectOnSpawnComponent, MapInitEvent>(OnInjectMapInit);
    }

    private void OnInjectMapInit(Entity<InjectOnSpawnComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<SolutionContainerManagerComponent>(ent.Owner, out var solContainer)
            || !HasComp<InjectableSolutionComponent>(ent.Owner))
            return;

        var entity = (ent.Owner, solContainer);

        if (!_solutionContainer.TryGetSolution(entity, ent.Comp.SolutionName, out var targetSln, out var targetSolution, true))
            return;

        var solution = new Solution();

        foreach (var reagent in ent.Comp.Reagents)
        {
            solution.AddReagent(reagent);
        }

        _solutionContainer.Inject(ent.Owner, targetSln.Value, solution);
        _reactive.DoEntityReaction(ent.Owner, solution, ReactionMethod.Touch);
    }
}
