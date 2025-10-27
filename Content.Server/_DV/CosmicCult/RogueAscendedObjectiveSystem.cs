using Content.Server._DV.CosmicCult.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Server._DV.CosmicCult;

public sealed class RogueAscendedObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize() =>
        SubscribeLocalEvent<RogueInfectionConditionComponent, ObjectiveGetProgressEvent>(OnGetInfectionProgress);

    private void OnGetInfectionProgress(EntityUid uid, RogueInfectionConditionComponent comp, ref ObjectiveGetProgressEvent args) =>
        args.Progress = _number.GetTarget(uid) == 0
            ? 1f
            : MathF.Min(comp.MindsCorrupted / (float) _number.GetTarget(uid), 1f);
}
