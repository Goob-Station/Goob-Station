using Content.Goobstation.Common.MartialArts;

namespace Content.Shared.Movement.Pulling.Events;

public sealed class CheckGrabOverridesEvent : EntityEventArgs
{
    public CheckGrabOverridesEvent(GrabStage stage)
    {
        Stage = stage;
    }

    public GrabStage Stage { get; set; }
}
