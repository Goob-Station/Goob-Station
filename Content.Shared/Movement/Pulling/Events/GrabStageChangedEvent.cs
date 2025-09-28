using Content.Goobstation.Common.MartialArts;

namespace Content.Shared.Movement.Pulling.Events;

public sealed class GrabStageChangedEvent : EntityEventArgs
{
    public GrabStageChangedEvent(GrabStage prev, GrabStage stage, EntityUid target)
    {
        Stage = stage;
        PrevStage = prev;
        Target = target;
    }

    public GrabStage PrevStage { get; set; }
    public GrabStage Stage { get; set; }
    public EntityUid Target { get; set; }
}
