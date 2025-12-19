using Content.Server.Tesla.Components;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Objectives;

/// <summary>
/// This handles...
/// </summary>
public sealed class DestroyTheTeslaObjectiveSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DestroyTheTeslaObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }
    private void OnGetProgress(EntityUid uid, DestroyTheTeslaObjectiveComponent component, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0f;
        var query = EntityQueryEnumerator<TeslaEnergyBallComponent>();
        while (query.MoveNext(out _, out _))
        {
            args.Progress = 0f;
            return;
        }

        args.Progress = 1f;
    }
}
