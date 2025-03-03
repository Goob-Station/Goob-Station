using Content.Shared.Movement.Pulling.Systems;

namespace Content.Shared._Goobstation.MartialArts.Components;

public abstract partial class GrabStagesOverrideComponent : Component
{
    public GrabStage StartingStage = GrabStage.Hard;
}
