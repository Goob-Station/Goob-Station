using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Gibbing.Events;

namespace Content.Goobstation.Shared.Gib;

public sealed class GibIgnoreSolutionContainersSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _soln = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SolutionContainerManagerComponent, AttemptEntityContentsGibEvent>(OnAttempt);
    }

    private void OnAttempt(Entity<SolutionContainerManagerComponent> ent, ref AttemptEntityContentsGibEvent args)
    {
        foreach (var container in _soln.EnumerateSolutionContainers(ent.AsNullable()))
        {
            args.ExcludedContainers ??= new();
            args.ExcludedContainers.Add(container);
        }
    }
}
