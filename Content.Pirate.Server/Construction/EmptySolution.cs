using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction;
using JetBrains.Annotations;

namespace Content.Pirate.Server.Construction;

/// <summary>
/// Empties a solution after construction.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class EmptySolution : IGraphAction
{
    [DataField(required: true)]
    public string Solution = string.Empty;

    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        var solutionSystem = entityManager.System<SharedSolutionContainerSystem>();
        if (solutionSystem.TryGetSolution(uid, Solution, out var soln, out _))
            solutionSystem.RemoveAllSolution(soln.Value);
    }
}
