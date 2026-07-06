using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction;
using JetBrains.Annotations;

namespace Content.Pirate.Server.Construction;

/// <summary>
/// Construction completion action that empties a named solution on the finished entity. Used so a
/// freshly built shower starts with an empty tank and refills over time, while mapped showers keep
/// the full tank they spawn with (mapped entities never traverse the build edge this runs on).
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
