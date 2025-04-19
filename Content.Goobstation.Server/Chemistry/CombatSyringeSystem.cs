using Content.Goobstation.Shared.DoAfter;
using Content.Server.Body.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Chemistry.Components;
using Robust.Server.Audio;

namespace Content.Goobstation.Server.Chemistry;

public sealed class CombatSyringeSystem : EntitySystem
{
    [Dependency] private readonly SolutionContainerSystem _solution = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InjectorComponent, CombatSyringeTriggerEvent>(OnCombatSyringeHit);
    }

    private void OnCombatSyringeHit(Entity<InjectorComponent> ent, ref CombatSyringeTriggerEvent args)
    {
        if (args.AffectedEntities == null || args.AffectedEntities.Count == 0)
            return;

        var target = args.AffectedEntities[0];

        if (TryComp(target, out BloodstreamComponent? bloodStream) && _solution.ResolveSolution(target,
                bloodStream.ChemicalSolutionName,
                ref bloodStream.ChemicalSolution,
                out var chemSolution))
        {
        }

        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out var soln, out var solution))
            return;

        _audio.PlayPvs(args.InjectSound, target);
        QueueDel(ent);
    }
}
