using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Mobs;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Chemistry.InjectOnMobState;

public sealed class InjectOnMobStateSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainers = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InjectOnMobStateComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(Entity<InjectOnMobStateComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != ent.Comp.State)
            return;

        if (ent.Comp.NextUse > _timing.CurTime)
            return;

        if (!HasComp<SolutionContainerManagerComponent>(ent))
            return;

        if (!_solutionContainers.TryGetInjectableSolution(ent.Owner, out var targetSoln, out var targetSolution))
            return;

        var solution = new Solution()
        {
            MaxVolume = targetSolution.AvailableVolume
        };

        foreach (var item in ent.Comp.Reagents)
        {
            if (targetSolution.Volume >= targetSolution.MaxVolume)
                break;

            solution.AddReagent(item.Key, Math.Min(item.Value.Float(), targetSolution.AvailableVolume.Float()));
        }

        _solutionContainers.TryAddSolution(targetSoln.Value, solution);
        ent.Comp.NextUse = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.Cooldown);
    }
}
